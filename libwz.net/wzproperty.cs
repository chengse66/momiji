using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class wzproperty : wzlist<wzproperty>
{
	private readonly static List<string> types = new List<string>() { "Shape2D#Convex2D", "Shape2D#Vector2D", "Sound_DX8", "Property", "Canvas", "UOL", };

	private object[] properties;

	public string identity
	{
		get
		{
			return properties[0] as string;
		}
	}

	public int type
	{
		get
		{
			return (int)properties[1];
		}
	}

	public int offset
	{
		get
		{
			return (int)properties[2];
		}
	}

	public object data
	{
		get
		{
			return properties[3];
		}
		internal set
		{
			properties[3] = value;
		}
	}

	public wzproperty host
	{
		get
		{
			return properties[4] as wzproperty;
		}
	}

	private wzreader reader
	{
		get
		{
			return properties[5] as wzreader;
		}
	}

	public string content
	{
		get
		{
			if (0 == type) // "Shape2D#Convex2D"
				return null;
			if (1 == type) // Shape2D#Vector2D
				return (data as wzvector).content;
			if (2 == type) // Sound_DX8
				return (data as wzaudio).content;
			if (3 == type) // "Property"
				return null;
			if (4 == type) // Canvas
				return (data as wzcanvas).content;
			if (5 == type) // UOL
				return (data as wzuol).link;
			if (14 == type) // 0x08
				return trim_content(data + "");

			return data + "";
		}
	}

	public wzproperty root
	{
		get
		{
			if (null != host)
				if (0 <= host.type)
					return host.root;

			return this;
		}
	}

	public string absolute
	{
		get
		{
			if (null != host)
				if (0 <= host.type)
					return host.absolute + "/" + identity;

			return identity;
		}
	}

	internal wzproperty(string identity, int type, int offset, object data, wzproperty host, wzreader reader)
	{
		properties = new object[]
		{
			identity,
			type,
			offset,
			data,
			host,
			reader,
		};
	}

	internal static int index_of_type(string type)
	{
		return types.IndexOf(type);
	}

	public T query<T>(T value, params string[] elements)
	{
		wzproperty property = this[elements];

		return null == property ? value : null == property.data ? value : (T)Convert.ChangeType(property.data, typeof(T));
	}

	public static T query<T>(wzproperty property, T value, params string[] sections)
	{
		return null == property ? value : property.query(value, sections);
	}

	private string trim_content(string content)
	{
		return (20 >= content.Length) ? content : (content.Substring(0, 20) + "...(" + content.Length + ")");
	}
}

public class wzvector
{
	public static wzvector operator +(wzvector a, wzvector b)
	{
		if (null == a)
			a = new wzvector(0, 0);
		if (null == b)
			b = new wzvector(0, 0);

		return new wzvector(a.x + b.x, a.y + b.y);
	}

	public static wzvector operator -(wzvector a, wzvector b)
	{
		if (null == a)
			a = new wzvector(0, 0);
		if (null == b)
			b = new wzvector(0, 0);

		return new wzvector(a.x - b.x, a.y - b.y);
	}

	private int[] properties;

	public int x
	{
		get
		{
			return properties[0];
		}
	}

	public int y
	{
		get
		{
			return properties[1];
		}
	}

	public string content
	{
		get
		{
			return properties[0] + "," + properties[1];
		}
	}

	public wzvector(int x, int y)
	{
		properties = new int[]
		{
			x,
			y,
		};
	}
}

public class wzaudio
{
	private readonly static int[, ,] bitrates =
	{
		//mpeg1
		{
			{ 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 0, }, // layer1
			{ 0, 32, 48, 56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 384, 0, }, // layer2
			{ 0, 32, 40, 48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 0, }, // layer3
		},
		//mpeg2 & 2.5
		{
			{ 0, 32, 48, 56,  64,  80,  96, 112, 128, 144, 160, 176, 192, 224, 256, 0, }, // layer1
			{ 0,  8, 16, 24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160, 0, }, // layer2
			{ 0,  8, 16, 24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160, 0, }, // layer3
		},
	};
	private readonly static int[,] frequencies =
	{
		{ 11025, 12000,  8000, 0, }, // mpeg2.5
		{     0,     0,     0, 0, }, // reserved
		{ 22050, 24000, 16000, 0, }, // mpeg2
		{ 44100, 48000, 32000, 0, }, // mpeg1
	};
	private readonly static int[,] samples_per_frames =
	{
		//mpeg1
		{
			 384, // layer1
			1152, // layer2
			1152, // layer3
		},
		//mpeg2 & 2.5
		{
			 384, // layer1
			1152, // layer2
			 576, // layer3
		},
	};
	private readonly static int[,] coefficients =
	{
		//mpeg1
		{
			 12, // layer1
			144, // layer2
			144, // layer3
		},
		//mpeg2 & 2.5
		{
			 12, // layer1
			144, // layer2
			 72, // layer3
		},
	};
	private readonly static int[] slots =
	{
		4, // layer1
		1, // layer2
		1, // layer3
	};

	private object[] properties;

	public int size
	{
		get
		{
			return (int)properties[1];
		}
	}

	public int offset
	{
		get
		{
			return (int)properties[3];
		}
	}

	private int header
	{
		get
		{
			return (int)properties[4];
		}
	}

	private wzreader reader
	{
		get
		{
			return properties[5] as wzreader;
		}
	}

	private byte[] wave_header
	{
		get
		{
			reader.position = offset - header;

			if (1 == reader.read<short>())
			{
				reader.position = offset - header;

				return reader.read_bytes(0x10);
			}

			if (!probe_region('k'))
				if (!probe_region('g'))
					if (!probe_region('x'))
						;

			reader.position = offset - header;

			return reader.decrypt_bytes(reader.read_bytes(0x10));
		}
	}

	public byte[] data
	{
		get
		{
			reader.position = offset;

			return reader.read_bytes(size);
		}
	}

	public string content
	{
		get
		{
			if (0x12 != header)
			{
				byte[] raw;
				int tag = 0;
				int id3 = 0;

				if (128 < size)
				{
					reader.position = offset + size - 128;

					raw = reader.read_bytes(3);

					if (0x54 == raw[0] && 0x41 == raw[1] && 0x47 == raw[2])
						tag = 128;
				}

				reader.position = offset;

				raw = reader.read_bytes(4);

				if (0x49 == raw[0] && 0x44 == raw[1] && 0x33 == raw[2])
				{
					reader.position = reader.position + 2;

					raw = reader.read_bytes(4);

					id3 = 10 + (((raw[0] & 0x7f) << 21) + ((raw[1] & 0x7f) << 14) + ((raw[2] & 0x7f) << 7) + (raw[3] & 0x7f));

					reader.position = offset + id3;

					raw = reader.read_bytes(4);
				}

				while (0xff == raw[0] && 0xe0 == (raw[1] & 0xe0) && 0xf0 != (raw[2] & 0xf0))
				{
					int version;
					int layer;
					int crc;
					int bitrate;
					int frequency;
					int padding;
					int framesize;/*
					int framescount;*/
					int length;

					// ---vv--- (0 = mpeg2.5, 1 = reserved, 2 = mpeg2, 3 = mpeg1)
					version = (raw[1] >> 3) & 3;

					if (1 == version)
						break;

					// -----ll- (0 = reserved, 1 = layer3, 2 = layer2, 3 = layer1)
					// 3 - layer
					layer = 3 - ((raw[1] >> 1) & 3);

					if (3 == layer)
						break;

					// -------c
					crc = raw[1] & 1;

					// bbbb----
					bitrate = (raw[2] >> 4) & 15;

					if (0 == bitrate)
						break;

					bitrate = bitrates[3 == version ? 0 : 1, layer, bitrate];

					// ----ff--
					frequency = (raw[2] >> 2) & 3;

					if (3 == frequency)
						break;

					frequency = frequencies[version, frequency];

					// ------p-
					padding = (raw[2] >> 1) & 1;

					framesize = (coefficients[3 == version ? 0 : 1, layer] * bitrate * 1000 / frequency + padding) * slots[layer];

					// song length...
					length = (int)(((size - tag - id3) / bitrate) * 8.0);

					return string.Format("{0:d2}:{1:d2}.{2,-4:d} {3,3:d}kbps {4:g}khz", length / 60000, (length / 1000) % 60, length % 1000, bitrate, frequency / 1000.0);
				}
			}
			else
			{
				byte[] raw = wave_header;
				double frequency = BitConverter.ToInt32(raw, 0x04) / 1000.0;
				int bitrate = BitConverter.ToInt16(raw, 0x0e);
				int ms = (int)((size / bitrate) * 8.0 / frequency);

				return string.Format("{0:d2}:{1:d2}.{2,-4:d} {3,3:d}kbps {4:g}khz", ms / 60000, (ms / 1000) % 60, ms % 1000, (int)(frequency * bitrate), frequency) + " : PCM";
			}

			return "Unknow format.";
		}
	}

	public bool pcm
	{
		get
		{
			return 0x12 == header;
		}
	}

	public string location
	{
		get
		{
			return reader.location;
		}
	}

	public byte[] wave
	{
		get
		{
			List<byte> stream = new List<byte>();

			stream.AddRange(Encoding.ASCII.GetBytes("RIFF"));
			stream.AddRange(BitConverter.GetBytes((int)(size + 44 - 8)));
			stream.AddRange(Encoding.ASCII.GetBytes("WAVE"));
			stream.AddRange(Encoding.ASCII.GetBytes("fmt "));
			stream.AddRange(BitConverter.GetBytes((int)(0x10)));
			stream.AddRange(wave_header);
			stream.AddRange(Encoding.ASCII.GetBytes("data"));
			stream.AddRange(BitConverter.GetBytes((int)(size)));
			stream.AddRange(data);

			return stream.ToArray();
		}
	}

	internal wzaudio(int unknow, int size, int unknow1, int offset, int header, wzreader reader)
	{
		properties = new object[]
		{
			unknow,
			size,
			unknow1,
			offset,
			header,
			reader,
		};
	}

	private bool probe_region(char region)
	{
		byte[] raw;

		reader.position = offset - header;
		reader.region = region;

		raw = reader.decrypt_bytes(reader.read_bytes(header));

		return 0x01 == raw[header - 0x12] && 0x00 == raw[header - 0x11];
	}

	public bool save(string location)
	{
		File.WriteAllBytes(location + (pcm ? ".wav" : ".mp3"), pcm ? wave : data);

		return true;
	}

	private void write_signature(string signture, Stream stream)
	{
		stream.Write(Encoding.ASCII.GetBytes(signture), 0, signture.Length);
	}

	private void write_int(int value, Stream stream)
	{
		stream.Write(BitConverter.GetBytes(value), 0, sizeof(int));
	}
}

public class wzcanvas
{
	private object[] properties;

	public int width
	{
		get
		{
			return (int)properties[1];
		}
	}

	public int height
	{
		get
		{
			return (int)properties[2];
		}
	}

	internal int format
	{
		get
		{
			return (int)properties[3];
		}
	}

	internal int size
	{
		get
		{
			return (int)properties[5];
		}
	}

	internal int offset
	{
		get
		{
			return (int)properties[6];
		}
	}

	private wzreader reader
	{
		get
		{
			return properties[7] as wzreader;
		}
	}

	internal string content
	{
		get
		{
			return string.Format("{0}x{1} : {2}", width, height, format);
		}
	}

	public byte[] data
	{
		get
		{
			reader.position = offset;

			return reader.read_bytes(size);
		}
	}

	public Image image
	{
		get
		{
			MemoryStream memory;
			DeflateStream deflate;
			int length;
			byte[] pixels;

			if (offset + size > reader.length)
				return null;

			length = width * height;

			switch (format)
			{
				case 0x0001:
				case 0x0003:
				case 0x0201: length = 2 * length; break;
				case 0x0002: length = 4 * length; break;
				case 0x0205: length = length / 8; break;
				case 0x0402:
				case 0x0802: break;
				default: return null;
			}

			reader.position = offset + 1;

			if (0x9c78 == reader.read<ushort>())
			{
				memory = new MemoryStream(reader.read_bytes(size - 3));
			}
			else
			{
				if (!probe_region('k'))
					if (!probe_region('g'))
						if (!probe_region('x'))
							return null;

				reader.position = offset + 1;

				memory = new MemoryStream();

				while (offset + size > reader.position)
				{
					int blocksize = reader.read<int>();

					memory.Write(reader.decrypt_bytes(reader.read_bytes(blocksize)), 0, blocksize);
				}

				memory.Position = 2;
			}

			deflate = new DeflateStream(memory, CompressionMode.Decompress);
			pixels = new byte[length];

			deflate.Read(pixels, 0, length);

			deflate.Close();
			memory.Close();

			return generate_image(pixels);
		}
	}

	internal wzcanvas(int unknow, int width, int height, int format, int reserved, int size, int offset, wzreader reader)
	{
		properties = new object[]
		{ 
			unknow,
			width,
			height,
			format,
			reserved,
			size,
			offset,
			reader,
		};
	}

	private bool probe_region(char region)
	{
		reader.position = offset + 5;
		reader.region = region;

		return 0x9c78 == BitConverter.ToUInt16(reader.decrypt_bytes(reader.read_bytes(2)), 0);
	}

	unsafe private Image generate_image(byte[] pixels)
	{
		Bitmap bitmap = new Bitmap(width, height, 0x0205 == format ? PixelFormat.Format1bppIndexed : 0x0201 == format ? PixelFormat.Format16bppRgb565 : PixelFormat.Format32bppArgb);
		BitmapData data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, bitmap.PixelFormat);
		byte* pointer = (byte*)data.Scan0;

		switch (format)
		{
			case 0x0001:

			case 0x0003:
				for (int index = 0, length = 2 * width * height; index < length; pointer = pointer + 4)
				{
					pointer[0] = (byte)((pixels[index] & 0x0f) + (pixels[index] << 0x04));
					pointer[1] = (byte)((pixels[index] & 0xf0) + (pixels[index++] >> 0x04));
					pointer[2] = (byte)((pixels[index] & 0x0f) + (pixels[index] << 0x04));
					pointer[3] = (byte)((pixels[index] & 0xf0) + (pixels[index++] >> 0x04));
				}
				break;

			case 0x0002: Marshal.Copy(pixels, 0, data.Scan0, data.Stride * height); break;

			case 0x0201:
				for (int index = 0, stride = 2 * width; index < height; ++index)
					Marshal.Copy(pixels, stride * index, new IntPtr((int)data.Scan0 + data.Stride * index), stride);
				break;

			case 0x0205:
				ColorPalette palette = bitmap.Palette;

				palette.Entries[0] = Color.FromArgb(0xff, 0x52, 0x86, 0xef);

				bitmap.Palette = palette;

				break;

			case 0x0402:
				fixed (byte* blocks = pixels)
					decompress((byte*)data.Scan0, width, height, blocks, dxt3);
				break;

			case 0x0802:
				fixed (byte* blocks = pixels)
					decompress((byte*)data.Scan0, width, height, blocks, dxt5);
				break;
		}

		bitmap.UnlockBits(data);

		return bitmap;
	}

	public bool save(string location)
	{
		Image png = image;

		png.Save(location + ".png", ImageFormat.Png);
		png.Dispose();

		return true;
	}

	unsafe private void decompress(byte* pixels, int width, int height, byte* blocks, int format)
	{
		int bytes_per_block = dxt1 != format ? 16 : 8;
		byte* target = stackalloc byte[64];
		byte* codes = stackalloc byte[16];
		byte* indices = stackalloc byte[16];

		for (int y = 0; y < height; y = y + 4)
			for (int x = 0; x < width; x = x + 4)
			{
				byte* block = dxt1 == format ? blocks : blocks + 8;

				int a = unpack565(codes, block);
				int b = unpack565(codes + 4, block + 2);

				for (int i = 0; i < 3; ++i)
				{
					int c = codes[i];
					int d = codes[4 + i];

					if (dxt1 == format && a <= b)
					{
						codes[8 + i] = (byte)((c + d) >> 1);
						codes[12 + i] = 0;
					}
					else
					{
						codes[8 + i] = (byte)((2 * c + d) / 3);
						codes[12 + i] = (byte)((2 * d + c) / 3);
					}
				}

				codes[8 + 3] = 255;
				codes[12 + 3] = (byte)((dxt1 == format && a <= b) ? 0 : 255);

				for (int i = 0; i < 4; ++i)
				{
					int ind = i << 2;
					byte packed = block[4 + i];

					indices[ind] = (byte)(packed & 0x3);
					indices[ind + 1] = (byte)((packed >> 2) & 0x3);
					indices[ind + 2] = (byte)((packed >> 4) & 0x3);
					indices[ind + 3] = (byte)((packed >> 6) & 0x3);
				}

				for (int i = 0; i < 16; ++i)
				{
					int offset = indices[i] << 2;

					for (int j = 0; j < 4; ++j)
						target[(i << 2) + j] = codes[offset + j];
				}

				block = blocks;

				#region alpha_dxt3
				if (dxt3 == format)
				{
					for (int i = 0; i < 8; ++i)
					{
						int lo = block[i] & 0x0f;
						int hi = block[i] & 0xf0;

						target[8 * i + 3] = (byte)(lo | (lo << 4));
						target[8 * i + 7] = (byte)(hi | (hi >> 4));
					}
				}
				#endregion
				#region alpha_dxt5
				else if (dxt5 == format)
				{
					byte alpha0 = block[0];
					byte alpha1 = block[1];

					block = block + 2;

					codes[0] = alpha0;
					codes[1] = alpha1;

					if (alpha0 <= alpha1)
					{
						for (int i = 1; i < 5; ++i)
							codes[i + 1] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);

						codes[6] = 0;
						codes[7] = 255;
					}
					else
					{
						for (int i = 1; i < 7; ++i)
							codes[1 + i] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
					}

					for (int i = 0; i < 2; ++i)
					{
						int value = 0;

						for (int j = 0; j < 3; ++j)
							value = value | (*block++ << 8 * j);

						for (int j = 0; j < 8; ++j)
							indices[i * 8 + j] = (byte)((value >> 3 * j) & 0x7);
					}

					for (int i = 0; i < 16; ++i)
						target[4 * i + 3] = codes[indices[i]];
				}
				#endregion

				for (int py = 0; py < 4; ++py)
					for (int px = 0; px < 4; ++px)
					{
						int sx = x + px;
						int sy = y + py;

						if (sx < width && sy < height)
							*((int*)pixels + (width * sy + sx)) = *(int*)target;

						target = target + 4;
					}

				target = target - 64;

				blocks = blocks + bytes_per_block;
			}
	}

	unsafe private int unpack565(byte* color, byte* packed)
	{
		int value = packed[0] | (packed[1] << 8);

		int red = (value >> 11) & 0x1f;
		int green = (value >> 5) & 0x3f;
		int blue = value & 0x1f;

		color[0] = (byte)((blue << 3) | (blue >> 2));
		color[1] = (byte)((green << 2) | (green >> 4));
		color[2] = (byte)((red << 3) | (red >> 2));
		color[3] = 255;

		return value;
	}

	private const int dxt1 = 1;
	private const int dxt3 = 2;
	private const int dxt5 = 4;
}

public class wzuol
{
	private object[] properties;

	public string link
	{
		get
		{
			return properties[1] as string;
		}
	}

	private wzproperty host
	{
		get
		{
			return properties[2] as wzproperty;
		}
	}

	public wzproperty target
	{
		get
		{
			wzproperty property = host.host;

			foreach (string identity in link.Split('/'))
				if (".." == identity)
				{
					property = property.host;
				}
				else
				{
					property = property[identity];

					if (null == property)
						return null;
				}

			return property;
		}
	}

	public wzuol(uint unknow, string link, wzproperty host)
	{
		properties = new object[]
		{
			unknow,
			link,
			host,
		};
	}
}
