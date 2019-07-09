
#define bit_stream_max_count 255

typedef struct {
	void *output;
	char stream[bit_stream_max_count];
	int count;
	int bits;
	int current_bits;
	int current_code;
} bit_stream;

void bit_stream_initial(int, void*, void*);
void bit_stream_append(int, void*);
void bit_stream_accomplish(void*);

int octal_tree_generate_palette(int*, int, unsigned char*, unsigned char*, int);

void floyd_steinberg_simple(unsigned char*, unsigned char*, int, unsigned char*, int);

void lzw_encoder_compress(unsigned char*, int, int, void*);
