using System;
using System.IO;

internal class wzreader : wzdecryptor
{
	private object[] properties;

	internal string location
	{
		get
		{
			return properties[0] as string;
		}
	}

	private Stream stream
	{
		get
		{
			return properties[1] as Stream;
		}
	}

	private BinaryReader reader
	{
		get
		{
			return properties[2] as BinaryReader;
		}
	}

	internal long length
	{
		get
		{
			return stream.Length;
		}
	}

	internal int position
	{
		get
		{
			return (int)stream.Position;
		}
		set
		{
			stream.Position = value;
		}
	}

	internal wzreader(string location)
	{
		Stream stream = File.OpenRead(location);

		properties = new object[]
		{
			location,
			stream,
			new BinaryReader(stream),
		};
	}

	internal void dispose()
	{
		reader.Close();
		stream.Close();
	}

	internal string transit_string(int offset)
	{
		switch (read<byte>())
		{
			case 0x04: position = position + 8; break;

			case 0x00:

			case 0x73: return decrypt_string();

			case 0x01:

			case 0x1b: return decrypt_string(offset + read<int>());
		}

		return "";
	}

	internal string decrypt_string(int offset)
	{
		string result;
		int register = position;

		position = offset;

		result = decrypt_string();

		position = register;

		return result;
	}

	internal string decrypt_string()
	{
		int size = read<sbyte>();

		if (0 < size)
			return decrypt_string16(read_bytes(((127 == size) ? read<int>() : size) * 2));
		else if (0 > size)
			return decrypt_string(read_bytes((-128 == size) ? read<int>() : -size));

		return "";
	}

	internal T upack<T>() where T : struct
	{
		sbyte value = read<sbyte>();

		if (-128 == value)
			return read<T>();

		return (T)Convert.ChangeType(value, typeof(T));
	}

	internal T read<T>() where T : struct
	{
		if (typeof(sbyte) == typeof(T))
			return (T)(object)reader.ReadSByte();
		if (typeof(byte) == typeof(T))
			return (T)(object)reader.ReadByte();
		if (typeof(short) == typeof(T))
			return (T)(object)reader.ReadInt16();
		if (typeof(ushort) == typeof(T))
			return (T)(object)reader.ReadUInt16();
		if (typeof(int) == typeof(T))
			return (T)(object)reader.ReadInt32();
		if (typeof(uint) == typeof(T))
			return (T)(object)reader.ReadUInt32();
		if (typeof(long) == typeof(T))
			return (T)(object)reader.ReadInt64();
		if (typeof(ulong) == typeof(T))
			return (T)(object)reader.ReadUInt64();
		if (typeof(float) == typeof(T))
			return (T)(object)reader.ReadSingle();
		if (typeof(double) == typeof(T))
			return (T)(object)reader.ReadDouble();

		return default(T);
	}

	internal byte[] read_bytes(int count)
	{
		return reader.ReadBytes(count);
	}
}
