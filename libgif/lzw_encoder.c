#include <malloc.h>
#include <memory.h>
#include <stdio.h>

#include "private.h"

#define lzw_encoder_max_bits 12
#define lzw_encoder_max_size (1 << lzw_encoder_max_bits)

void lzw_encoder_write_byte(int byte, void *stream) {
	fwrite(&byte, sizeof(char), 1, stream);
}

void lzw_encoder_reset_code_table(int **code_table) {
	int index;

	for (index = 0; 256 > index; ++index)
		memset(code_table[index], 0xff, sizeof(int) * lzw_encoder_max_size);
}

void lzw_encoder_compress(unsigned char *indices, int length, int depth, void *stream) {
	int prefix, surfix, offset = 0;
	int current_depth = depth;
	int clear_flag = 1 << current_depth;
	int end_flag = clear_flag + 1;
	int code_bits = current_depth + 1;
	int current_max_size = 1 << code_bits;
	int available_code = end_flag + 1;
	int *code_table[256];
	bit_stream output;

	for (prefix = 0; 256 > prefix; ++prefix)
		code_table[prefix] = calloc(lzw_encoder_max_size, sizeof(int));

	lzw_encoder_reset_code_table(code_table);

	bit_stream_initial(code_bits, stream, &output);
	bit_stream_append(clear_flag, &output);

	lzw_encoder_write_byte(depth, stream);

	surfix = indices[offset++];

	while (length > offset) {
		prefix = surfix;
		surfix = indices[offset++];

		if (-1 == code_table[surfix][prefix]) {
			bit_stream_append(prefix, &output);

			code_table[surfix][prefix] = available_code++;

			if (lzw_encoder_max_size <= available_code) {
				lzw_encoder_reset_code_table(code_table);

				current_depth = depth;
				code_bits = current_depth + 1;
				current_max_size = 1 << code_bits;
				available_code = end_flag + 1;

				bit_stream_append(clear_flag, &output);

				output.bits = code_bits;
			} else if (current_max_size < available_code) {
				current_depth = current_depth + 1;
				code_bits = current_depth + 1;
				current_max_size = 1 << code_bits;

				output.bits = code_bits;
			}
		} else {
			surfix = code_table[surfix][prefix];
		}
	}

	bit_stream_append(surfix, &output);
	bit_stream_append(end_flag, &output);
	bit_stream_accomplish(&output);

	lzw_encoder_write_byte(0, stream);

	for (prefix = 0; 256 > prefix; ++prefix)
		free(code_table[prefix]);
}
