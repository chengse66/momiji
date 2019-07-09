
#define gif_encoder_base struct { \
	void (__stdcall *destruct)(void*); \
	void (__stdcall *append_frame)(int*, int, void*); \
}

typedef gif_encoder_base gif_encoder;

gif_encoder* gif_encoder_construct(void*, int, int, int, int);
