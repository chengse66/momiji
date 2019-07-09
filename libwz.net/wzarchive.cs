
public class wzarchive
{
	private object[] properties;

	private wzreader reader
	{
		get
		{
			return properties[0] as wzreader;
		}
	}

	private wzheader header
	{
		get
		{
			return properties[1] as wzheader;
		}
	}

	internal int version
	{
		get
		{
			return header.version;
		}
	}

	public wzpackage root
	{
		get
		{
			if (header.valid)
				if (probe_region())
				{
					reader.position = header.size + 2;
					header.eod = compute_eod();
					reader.position = header.size + 2;

					return expand(new wzpackage(null, 0, 0, 0, 0, null, null));
				}

			dispose();

			return null;
		}
	}

	public wzarchive(string location)
	{
		wzreader reader = new wzreader(location);
		wzheader header = new wzheader(reader);

		properties = new object[] { reader, header, };
	}

	public void dispose()
	{
		reader.dispose();
	}

	private bool probe_region()
	{
		foreach (char region in new char[] { 'k', 'g', 'x', })
		{
			reader.position = header.size + 2;
			reader.region = region;

			if (query_identity().EndsWith(".img"))
				return true;
		}

		return false;
	}

	private string query_identity()
	{
		int children = 0;

		for (int count = reader.upack<int>(); count > 0; --count)
		{
			switch (reader.read<byte>())
			{
				case 1: reader.decrypt_string(header.size + 1 + reader.read<int>()); ++children; break;

				case 2: return reader.decrypt_string(header.size + 1 + reader.read<int>());

				case 3: reader.decrypt_string(); ++children; break;

				case 4: return reader.decrypt_string();

				default: return null;
			}

			reader.upack<int>();
			reader.upack<int>();
			reader.read<int>();
		}

		while (0 < children--)
		{
			string identity = query_identity();

			if (null != identity)
				return identity;
		}

		return null;
	}

	private int compute_eod()
	{
		int children = 0;

		for (int count = reader.upack<int>(); count > 0; --count)
		{
			switch (reader.read<byte>())
			{
				case 1: reader.decrypt_string(header.size + 1 + reader.read<int>()); ++children; break;

				case 2: reader.decrypt_string(header.size + 1 + reader.read<int>()); break;

				case 3: reader.decrypt_string(); ++children; break;

				case 4: reader.decrypt_string(); break;

				default: return 0;
			}

			reader.upack<int>();
			reader.upack<int>();
			reader.read<int>();
		}

		while (0 < children--)
			compute_eod();

		return reader.position;
	}

	private wzpackage expand(wzpackage host)
	{
		for (int count = reader.upack<int>(); count > 0; --count)
		{
			string identity;
			byte type = reader.read<byte>();

			switch (type)
			{
				case 1:

				case 2: identity = reader.decrypt_string(header.size + 1 + reader.read<int>()); break;

				case 3:

				case 4: identity = reader.decrypt_string(); break;

				default: return null;
			}

			host.append(identity, new wzpackage(identity, type, reader.upack<int>(), reader.upack<int>(), header.compute_offset(), host, reader));
		}

		foreach (wzpackage child in host)
			if (0 != child.type % 2)
				if (null == expand(child))
					return null;

		return host;
	}
}
