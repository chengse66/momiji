#include <memory.h>
#include <stdio.h>

#include "private.h"

void bit_stream_initial(int initial, void *output, bit_stream *stream) {
	memset(stream, 0, sizeof(bit_stream));

	stream->output = output;
	stream->bits = initial;
}

void bit_stream_write(bit_stream *stream) {
	fwrite(&stream->count, sizeof(char), 1, stream->output);
	fwrite(stream->stream, sizeof(char), stream->count, stream->output);

	memset(stream->stream, 0, sizeof(char) * bit_stream_max_count);

	stream->count = 0;
}

void bit_stream_attach(bit_stream *stream) {
	stream->stream[stream->count++] = stream->current_code & 0xff;

	stream->current_code = stream->current_code >> 8;
	stream->current_bits = stream->current_bits - 8;

	if (bit_stream_max_count <= stream->count)
		bit_stream_write(stream);
}

void bit_stream_append(int code, bit_stream *stream) {
	stream->current_code = stream->current_code | (code << stream->current_bits);
	stream->current_bits = stream->current_bits + stream->bits;

	while (8 <= stream->current_bits)
		bit_stream_attach(stream);
}

void bit_stream_accomplish(bit_stream *stream) {
	while (0 < stream->current_bits)
		bit_stream_attach(stream);

	if (stream->count)
		bit_stream_write(stream);
}
