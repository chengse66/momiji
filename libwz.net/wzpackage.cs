using System.Collections.Generic;

public class wzpackage : wzlist<wzpackage>
{
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

	public int size
	{
		get
		{
			return (int)properties[2];
		}
	}

	public int sum32
	{
		get
		{
			return (int)properties[3];
		}
	}

	public int offset
	{
		get
		{
			return (int)properties[4];
		}
	}

	public wzpackage host
	{
		get
		{
			return properties[5] as wzpackage;
		}
		internal set
		{
			properties[5] = value;
		}
	}

	private wzreader reader
	{
		get
		{
			return properties[6] as wzreader;
		}
	}

	public bool intact
	{
		get
		{
			int sum = 0;

			reader.position = offset;

			foreach (byte num in reader.read_bytes(size))
				sum = sum + num;

			return sum32 == sum;
		}
	}

	public wzproperty root
	{
		get
		{
			if (2 == type || 4 == type)
				if (probe_region())
				{
					wzproperty property = new wzproperty(identity, -1, 0, null, null, null);

					reader.position = offset;

					if (expand_package("", offset, 0, property))
						return property;
				}

			return null;
		}
	}

	public string absolute
	{
		get
		{
			if (null != host)
				if (0 != host.type)
					return host.absolute + "/" + identity;

			return identity;
		}
	}

	internal wzpackage(string identity, int type, int size, int sum32, int offset, wzpackage host, wzreader reader)
	{
		properties = new object[]
		{
			identity,
			type,
			size,
			sum32,
			offset,
			host,
			reader,
		};
	}

	public wzproperty query(params string[] elements)
	{
		wzpackage package = this[elements];

		return null == package ? null : package.root[""];
	}

	private bool probe_region()
	{
		if (probe_identity('k'))
			return true;
		if (probe_identity('g'))
			return true;
		if (probe_identity('x'))
			return true;

		return false;
	}

	private bool probe_identity(char region)
	{
		reader.position = offset;
		reader.region = region;

		return 0 <= wzproperty.index_of_type(reader.transit_string(offset));
	}

	private bool expand_package(string identity, int offset, int eob/*end of block*/, wzproperty host)
	{
		string type = reader.transit_string(offset);
		int index = wzproperty.index_of_type(type);
		wzproperty property = new wzproperty(identity, index, reader.position, null, host, reader);

		host.append(identity, property);

		switch (index)
		{
			case 0: return expand_shape2d_Convex2D(offset, eob, property);

			case 1: return expand_shape2d_vector2d(property);

			case 2: return expand_sound_dx8(eob, property);

			case 3: return expand_property(offset, property);

			case 4: return expand_canvas(offset, property);

			case 5: return expand_uol(offset, property);

			default: break;
		}

		return false;
	}

	private bool expand_shape2d_Convex2D(int offset, int eob, wzproperty property)
	{
		int count = reader.upack<int>();

		for (int index = 0; index < count; ++index)
			if (!expand_package(index.ToString(), offset, eob, property))
				return false;

		return true;
	}

	private bool expand_shape2d_vector2d(wzproperty property)
	{
		property.data = new wzvector(reader.upack<int>(), reader.upack<int>());

		return true;
	}

	private bool expand_sound_dx8(int eob, wzproperty property)
	{
		int unknow = reader.read<byte>();
		int size = reader.upack<int>();
		int unknow1 = reader.upack<int>();

		reader.position = reader.position + 51;

		property.data = new wzaudio(unknow, size, unknow1, eob - size, reader.read<byte>(), reader);

		reader.position = eob;

		return true;
	}

	private bool expand_property(int offset, wzproperty property)
	{
		int unknow = reader.read<short>();
		int count = reader.upack<int>();

		for (int index = 0; index < count; ++index)
			if (!expand_block(offset, property))
				return false;

		property.data = unknow;

		return true;
	}

	private bool expand_canvas(int offset, wzproperty property)
	{
		wzcanvas canvas;
		int unknow = reader.read<byte>();

		if (1 == reader.read<byte>())
			if (!expand_property(offset, property))
				return false;

		canvas = new wzcanvas(unknow, reader.upack<int>(), reader.upack<int>(), reader.upack<int>() + reader.read<byte>(), reader.read<int>(), reader.read<int>(), reader.position, reader);

		property.data = canvas;

		reader.position = reader.position + canvas.size;

		return true;
	}

	private bool expand_uol(int offset, wzproperty property)
	{
		property.data = new wzuol(reader.read<byte>(), reader.transit_string(offset), property);

		return true;
	}

	private bool expand_block(int offset, wzproperty host)
	{
		object data;
		string identity = reader.transit_string(offset);
		int type = reader.read<byte>();
		int position = reader.position;

		switch (type)
		{
			case 0x00: data = null; break;

			case 0x02: goto case 0x0b;

			case 0x03: goto case 0x13;

			case 0x04: data = reader.upack<float>(); break;

			case 0x05: data = reader.read<double>(); break;

			case 0x08: data = reader.transit_string(offset); break;

			case 0x09: return expand_package(identity, offset, reader.read<int>() + reader.position, host);

			case 0x0b: data = reader.read<short>(); break;

			case 0x13: data = reader.upack<int>(); break;

			case 0x14: data = reader.upack<long>(); break;

			default: return false;
		}

		host.append(identity, new wzproperty(identity, type + 6, position, data, host, reader));

		return true;
	}
}

public class wzlist<T> where T : wzlist<T>
{
	private object[] properties;

	public List<string> identities
	{
		get
		{
			return properties[0] as List<string>;
		}
	}

	public List<T> values
	{
		get
		{
			return properties[1] as List<T>;
		}
	}

	public int count
	{
		get
		{
			return identities.Count;
		}
	}

	public T this[params string[] elements]
	{
		get
		{
			T value = this as T;

			foreach (string element in elements)
				if (value.identities.Contains(element))
					value = value.values[value.identities.IndexOf(element)];
				else
					return null;

			return value;
		}
	}

	internal wzlist()
	{
		properties = new object[] { new List<string>(), new List<T>(), };
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int index = 0; index < identities.Count; ++index)
			yield return values[index];
	}

	public bool query(out T value, params string[] elements)
	{
		value = this[elements];

		return null != value;
	}

	internal void append(string identity, T value)
	{
		identities.Add(identity);
		values.Add(value);
	}
}
