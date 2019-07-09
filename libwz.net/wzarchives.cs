using System.IO;
using System.Collections.Generic;

public class wzarchives
{
	private object[] properties;

	public wzpackage root
	{
		get
		{
			return properties[0] as wzpackage;
		}
	}

	private List<wzarchive> archives
	{
		get
		{
			return properties[1] as List<wzarchive>;
		}
	}

	public int version
	{
		get
		{
			return archives[0].version;
		}
	}

	public wzarchives(string location)
	{
		wzarchive archive = new wzarchive(location);
		wzpackage root = archive.root;

		if (null != root)
		{
			string prefix = Path.GetDirectoryName(location) + "\\";
			List<wzarchive> archives = new List<wzarchive>() { archive, };

			properties = new object[] { root, archives, };

			foreach (wzpackage property in root)
				if (1 == property.type % 2 && 0 == property.count)
				{
					location = prefix + property.identity + ".wz";

					if (File.Exists(location))
					{
						archive = new wzarchive(location);
						root = archive.root;

						if (null != root)
						{
							foreach (wzpackage child in root)
							{
								child.host = property;

								property.append(child.identity, child);
							}

							archives.Add(archive);
						}
					}
				}
		}
	}

	public void dispose()
	{
		foreach (wzarchive archive in archives)
			archive.dispose();
	}
}
