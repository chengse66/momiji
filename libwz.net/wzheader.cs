
internal class wzheader
{
	private object[] properties;

	private int singnature;
	private long datasize;
	private int headersize;
	private byte[] copyright;
	private ushort versionhash;

	private int[] versions;
	private uint[] factors;
	private int conclusion;
	private bool computed;

	internal int eod // end of directory
	{
		get
		{
			return (int)properties[0];
		}
		set
		{
			properties[0] = value;
		}
	}

	private wzreader reader
	{
		get
		{
			return properties[1] as wzreader;
		}
	}

	internal int size
	{
		get
		{
			return headersize;
		}
	}

	internal int version
	{
		get
		{
			return versions[conclusion];
		}
	}

	internal bool valid
	{
		get
		{
			if (64 <= reader.length)
			{
				singnature = reader.read<int>();
				datasize = reader.read<long>();
				headersize = reader.read<int>();
				copyright = reader.read_bytes(44);
				versionhash = reader.read<ushort>();

				if (0x31474b50 == singnature && reader.length == datasize + headersize)
				{
					versions = new int[10];
					factors = new uint[10];

					for (int version = 0, index = 0; index < 10; ++version)
					{
						uint factor = 0;

						foreach (char c in version.ToString())
							factor = (factor * 32) + c + 1;

						if (((((0xff ^
							((factor >> 0x18) & 0xff)) ^
							((factor >> 0x10) & 0xff)) ^
							((factor >> 0x08) & 0xff)) ^
							((factor >> 0x00) & 0xff)) == versionhash)
						{
							versions[index] = version;
							factors[index] = factor;

							++index;
						}
					}

					return true;
				}
			}

			return false;
		}
	}

	internal wzheader(wzreader reader)
	{
		properties = new object[]
		{
			0,
			reader,
		};
	}

	internal int compute_offset()
	{
		uint value = reader.read<uint>();
		uint offset = (uint)(reader.position - headersize - 4) ^ 0xffffffff;

		if (computed)
			offset = compute_offset(offset, factors[conclusion], value);
		else
			for (int index = 0; index < 10; ++index)
			{
				uint position = compute_offset(offset, factors[index], value);

				if (eod == position)
				{
					offset = position;
					conclusion = index;
					computed = true;

					break;
				}
			}

		return (int)offset;
	}

	private uint compute_offset(uint offset, uint factor, uint value)
	{
		offset = offset * factor - 0x581c3f6d;
		factor = offset & 0x1f;

		return (((offset << (int)factor) | (offset >> (0x20 - (int)factor))) ^ value) + 0x78;
	}
}
