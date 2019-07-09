#include <malloc.h>
#include <memory.h>

#define floyd_steinberg_clamp(v) if (0 > v) v = 0; else if (255 < v) v = 255;

int floyd_steinberg_closest_color(int red, int green, int blue, unsigned char *palette, int available) {
	int r, g, b, index, distance, closest = 0;
	int least = 0x0002fa04; // 0x7fffffff

	for (index = 0; available > index; ++index) {
		r = red - palette[0];
		g = green - palette[1];
		b = blue - palette[2];

		distance = r * r + g * g + b * b;

		if (least > distance) {
			least = distance;
			closest = index;

			if (!distance)
				break;
		}

		palette = palette + 3;
	}

	return closest;
}

void floyd_steinberg_error_diffusion(int *pixels, unsigned char *indices, int width, int height, int level, unsigned char *palette, int last_index) {
	unsigned char *source, *destination, *color;
	int *(thisrow[3]), *(nextrow[3]), error[4][4];
	int x, y, index, red, green, blue, delta, closest, stride = width + 1;

	for (index = 0; 3 > index; ++index) {
		thisrow[index] = calloc(stride, sizeof(int));
		nextrow[index] = calloc(stride, sizeof(int));
	}

	for (y = 0; height > y; ++y) {
		if (y & 1) {
			source = (void*)(pixels + width - 1);
			destination = indices + width - 1;
			delta = -1;
		} else {
			source = (void*)pixels;
			destination = indices;
			delta = +1;
		}

		for (x = 0; width > x; ++x) {
			blue = source[0] - ((thisrow[0][x] * level) / 100);
			green = source[1] - ((thisrow[1][x] * level) / 100);
			red = source[2] - ((thisrow[2][x] * level) / 100);

			closest = source[3] ? floyd_steinberg_closest_color(red, green, blue, palette, last_index) : last_index;
			color = palette + 3 * closest;

			destination[0] = closest;

			error[0][0] = color[2] - blue;
			error[0][1] = color[1] - green;
			error[0][2] = color[0] - red;

			error[1][0] = (error[0][0] * 7) / 16;
			error[1][1] = (error[0][0] * 5) / 16;
			error[1][2] = (error[0][0] * 3) / 16;
			error[1][3] = error[0][0] - error[1][0] - error[1][1] - error[1][2];

			error[2][0] = (error[0][1] * 7) / 16;
			error[2][1] = (error[0][1] * 5) / 16;
			error[2][2] = (error[0][1] * 3) / 16;
			error[2][3] = error[0][1] - error[2][0] - error[2][1] - error[2][2];

			error[3][0] = (error[0][2] * 7) / 16;
			error[3][1] = (error[0][2] * 5) / 16;
			error[3][2] = (error[0][2] * 3) / 16;
			error[3][3] = error[0][2] - error[3][0] - error[3][1] - error[3][2];

			index = x + 1;

			thisrow[0][index] = thisrow[0][index] + error[1][0];
			thisrow[1][index] = thisrow[1][index] + error[2][0];
			thisrow[2][index] = thisrow[2][index] + error[3][0];

			index = width - x;

			nextrow[0][index] = nextrow[0][index] + error[1][1];
			nextrow[1][index] = nextrow[1][index] + error[2][1];
			nextrow[2][index] = nextrow[2][index] + error[3][1];

			if (x) {
				index= width - x + 1;

				nextrow[0][index] = nextrow[0][index] + error[1][2];
				nextrow[1][index] = nextrow[1][index] + error[2][2];
				nextrow[2][index] = nextrow[2][index] + error[3][2];
			}

			index = width - x - 1;

			nextrow[0][index] = nextrow[0][index] + error[1][3];
			nextrow[1][index] = nextrow[1][index] + error[2][3];
			nextrow[2][index] = nextrow[2][index] + error[3][3];

			source = (void*)((int*)source + delta);
			destination = destination + delta;
		}

		pixels = pixels + width;
		indices = indices + width;

		for (index = 0; 3 > index; ++index) {
			memcpy(thisrow[index], nextrow[index], sizeof(int) * stride);
			memset(nextrow[index], 0, sizeof(int) * stride);
		}
	}

	for (index = 0; 3 > index; ++index) {
		free(thisrow[index]);
		free(nextrow[index]);
	}
}

void floyd_steinberg_simple(unsigned char *pixels, unsigned char *indices, int length, unsigned char *palette, int available) {
	while (length--) {
		indices[0] = pixels[3] ? floyd_steinberg_closest_color(pixels[2], pixels[1], pixels[0], palette, available) : available;

		++(int*)pixels;
		++indices;
	}
}
