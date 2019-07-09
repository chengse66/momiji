#include <malloc.h>
#include <math.h>
#include <memory.h>
#include <stdio.h>
#include <tchar.h>

#include "libgif.h"
#include "private.h"

#pragma pack(1)
typedef struct {
	unsigned char signature[3];							// GIF
	unsigned char version[3];							// 89a
} gif_encoder_header;

typedef struct {
	unsigned short logical_screen_width;
	unsigned short logical_screen_height;
	unsigned char size_of_global_color_table:3;
	unsigned char sort_flag:1;
	unsigned char color_resolution:3;
	unsigned char global_color_table_flag:1;
	unsigned char background_color_index;
	unsigned char pixel_aspect_ratio;
} gif_encoder_logical_screen_descriptor;

typedef struct {
	unsigned char extension_introducer;					// 0x21
	unsigned char extension_label;						// 0xff
	unsigned char block_size;							// 0x0b
	unsigned char application_identifier[8];			// NETSCAPE
	unsigned char application_authentication_code[3];	// 2.0
	unsigned int application_data;						// 0x00000103
	unsigned char block_terminator;						// 0x00
} gif_encoder_application_extension;

typedef struct {
	unsigned char extension_introducer;					// 0x21
	unsigned char graphic_control_label;				// 0xf9
	unsigned char block_size;							// 0x04
	unsigned char transparent_color_flag:1;
	unsigned char user_input_flag:1;
	unsigned char disposal_method:3;
	unsigned char reserved:3;
	unsigned short delay_time;
	unsigned char transparent_color_index;
	unsigned char block_terminator;						// 0x00
} gif_encoder_graphic_control_extension;

typedef struct {
	unsigned char image_separator;						// 0x2c
	unsigned short image_left_position;
	unsigned short image_top_position;
	unsigned short image_width;
	unsigned short image_height;
	unsigned char size_of_local_color_table:3;
	unsigned char reserved:2;
	unsigned char sort_flag:1;
	unsigned char interlace_flag:1;
	unsigned char local_color_table_flag:1;
} gif_encoder_image_descriptor;

typedef struct {
	unsigned char gif_encoder_trailer;					// 0x3b
} gif_encoder_trailer;
#pragma pack()

typedef struct {
	gif_encoder_base;
	void *stream;
	int width;
	int height;
	int max_color;
	union {
		struct {
			unsigned char blue;
			unsigned char green;
			unsigned char red;
			unsigned char alpha;
		};
		int back_color;
	};
	unsigned char palette[3 * 256];
	void *destination;
	unsigned char *indices;
	void (__stdcall *blend_function)(unsigned char*, unsigned char*, int, void*);
} gif_encoder_context;

void gif_encoder_destruct(void*);
void gif_encoder_append_frame(int*, int, void*);

void gif_encoder_write_header(gif_encoder_context *encoder) {
	gif_encoder_header data = { 'G', 'I', 'F', '8', '9', 'a', };

	fwrite(&data, sizeof(gif_encoder_header), 1, encoder->stream);
}

void gif_encoder_write_logical_screen_descriptor(gif_encoder_context *encoder) {
	gif_encoder_logical_screen_descriptor data;

	memset(&data, 0, sizeof(gif_encoder_logical_screen_descriptor));

	data.logical_screen_width = (unsigned short)encoder->width;
	data.logical_screen_height = (unsigned short)encoder->height;

	fwrite(&data, sizeof(gif_encoder_logical_screen_descriptor), 1, encoder->stream);
}

void gif_encoder_write_application_extension(gif_encoder_context *encoder) {
	gif_encoder_application_extension data = { 0x21, 0xff, 0x0b, 'N', 'E', 'T', 'S', 'C', 'A', 'P', 'E', '2', '.', '0', 0x00000103, 0x00, };

	fwrite(&data, sizeof(gif_encoder_application_extension), 1, encoder->stream);
}

void gif_encoder_write_graphic_control_extension(int delay, int index, gif_encoder_context *encoder) {
	gif_encoder_graphic_control_extension data;

	memset(&data, 0, sizeof(gif_encoder_graphic_control_extension));

	data.extension_introducer = 0x21;
	data.graphic_control_label = 0xf9;
	data.block_size = 0x04;
	data.transparent_color_flag = encoder->alpha ? 0 : 1;
	data.disposal_method = 2;
	data.delay_time = (unsigned short)(delay / 10);
	data.transparent_color_index = index;

	fwrite(&data, sizeof(gif_encoder_graphic_control_extension), 1, encoder->stream);
}

void gif_encoder_write_image_descriptor(int depth, gif_encoder_context *encoder) {
	gif_encoder_image_descriptor data;

	memset(&data, 0, sizeof(gif_encoder_image_descriptor));

	data.image_separator = 0x2c;
	data.image_width = (unsigned short)encoder->width;
	data.image_height = (unsigned short)encoder->height;
	data.size_of_local_color_table = depth - 1;
	data.local_color_table_flag = 1;

	fwrite(&data, sizeof(gif_encoder_image_descriptor), 1, encoder->stream);
}

void gif_encoder_write_trailer(gif_encoder_context *encoder) {
	gif_encoder_trailer data = { 0x3b, };

	fwrite(&data, sizeof(gif_encoder_trailer), 1, encoder->stream);
}

void gif_encoder_blend_white(unsigned char *destination, unsigned char *pixels, int length, gif_encoder_context *encoder) {
	double alpha;
	int white;

	while (length--) {
		if (pixels[3]) {
			alpha = pixels[3] / 255.0;
			white = 255 - pixels[3];

			destination[0] = (unsigned char)(pixels[0] * alpha + white);
			destination[1] = (unsigned char)(pixels[1] * alpha + white);
			destination[2] = (unsigned char)(pixels[2] * alpha + white);
			destination[3] = 255;
		} else {
			*(int*)destination = 0;
		}

		++(int*)destination;
		++(int*)pixels;
	}
}

void gif_encoder_blend_color(unsigned char *destination, unsigned char *pixels, int length, gif_encoder_context *encoder) {
	double alpha, beta;
	unsigned char red, green, blue;

	red = encoder->red;
	green = encoder->green;
	blue = encoder->blue;

	while (length--) {
		if (pixels[3]) {
			alpha = pixels[3] / 255.0;
			beta = 1.0 - alpha;

			destination[0] = (unsigned char)(pixels[0] * alpha + blue * beta);
			destination[1] = (unsigned char)(pixels[1] * alpha + green * beta);
			destination[2] = (unsigned char)(pixels[2] * alpha + red * beta);
			destination[3] = 255;
		} else {
			*(int*)destination = 0;
		}

		++(int*)destination;
		++(int*)pixels;
	}
}

gif_encoder* gif_encoder_construct(TCHAR *location, int width, int height, int max_color, int back_color) {
	gif_encoder_context *encoder = calloc(1, sizeof(gif_encoder_context));

	encoder->destruct = gif_encoder_destruct;
	encoder->append_frame = gif_encoder_append_frame;
	encoder->stream = _tfopen(location, _T("wb"));
	encoder->width = width;
	encoder->height = height;
	encoder->max_color = max_color;
	encoder->back_color = back_color;
	encoder->destination = calloc(width * height, sizeof(int));
	encoder->indices = calloc(width * height, sizeof(char));
	encoder->blend_function = encoder->alpha ? gif_encoder_blend_color : gif_encoder_blend_white;

	gif_encoder_write_header(encoder);
	gif_encoder_write_logical_screen_descriptor(encoder);
	gif_encoder_write_application_extension(encoder);

	return (void*)encoder;
}

void gif_encoder_destruct(gif_encoder_context *encoder) {
	gif_encoder_write_trailer(encoder);

	fclose(encoder->stream);

	free(encoder->destination);
	free(encoder->indices);
	free(encoder);
}

void gif_encoder_append_frame(int *pixels, int delay, gif_encoder_context *encoder) {
	int actual, depth, length = encoder->width * encoder->height;

	encoder->blend_function(encoder->destination, (void*)pixels, length, encoder);

	actual = octal_tree_generate_palette(encoder->destination, length, encoder->palette, &encoder->blue, encoder->max_color);
	depth = (int)ceil(log(actual) / log(2));

	gif_encoder_write_graphic_control_extension(delay, actual - 1, encoder);
	gif_encoder_write_image_descriptor(depth, encoder);

	fwrite(encoder->palette, sizeof(char), 3 * (1 << depth), encoder->stream);

	floyd_steinberg_simple(encoder->destination, encoder->indices, length, encoder->palette, actual - 1);

	lzw_encoder_compress(encoder->indices, length, depth, encoder->stream);

	memset(encoder->palette, 0, sizeof(char) * 3 * actual);
}
