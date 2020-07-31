using System;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

internal class manager : Form
{
	private static XmlAttribute generate_attribute(string name, string value, XmlDocument document)
	{
		XmlAttribute attribute = document.CreateAttribute(name);

		attribute.Value = value;

		return attribute;
	}

	private const string cash_negative = "0";
	private const string gender_neutral = "2";
	private const string job_neutral = "0";

	private TextBox locations;
	private ProgressBar archive;
	private ProgressBar total;
	private Button button;

	protected Thread thread;
	protected Thread reserve;

	private wzpackage packages;

	private string thumbnails
	{
		get
		{
			return momiji_host.assembly + "\\" + "thumbnails" + "\\";
		}
	}

	internal manager()
	{
		locations = new TextBox();
		archive = new ProgressBar();
		total = new ProgressBar();
		button = new Button();

		locations.Multiline = true;
		locations.Location = new Point(5, 5);
		locations.Size = new Size(290, 175);
		locations.Parent = this;

		locations.Click += locations_click;

		archive.Location = new Point(5, 185);
		archive.Size = new Size(265, 10);
		archive.Parent = this;

		total.Location = new Point(5, 195);
		total.Size = new Size(265, 10);
		total.Parent = this;

		button.Location = new Point(275, 185);
		button.Size = new Size(20, 20);
		button.Text = "o";
		button.Parent = this;

		button.Click += button_click;

		MaximizeBox = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterScreen;
		ClientSize = new Size(300, 210);
		Text = "resources manager";
	}

	protected override void Dispose(bool disposing)
	{
		if (null != reserve)
		{
			thread = null;

			while (reserve.IsAlive)
				Application.DoEvents();
		}

		base.Dispose(disposing);
	}

	private void locations_click(object o, EventArgs e)
	{
		using (FolderBrowserDialog folder = new FolderBrowserDialog())
		{
			folder.SelectedPath = Directory.GetCurrentDirectory();

			if (DialogResult.OK == folder.ShowDialog())
			{
				Directory.SetCurrentDirectory(folder.SelectedPath);

				locations.Text = locations.Text + folder.SelectedPath + "\r\n";
			}
		}
	}

	private void button_click(object o, EventArgs e)
	{
		string[] locationss;

		if (null != reserve)
		{
			if (null != thread)
				thread = null;
			if (reserve.IsAlive)
				return;
		}

		locationss = locations.Text.Split(new string[] { "\r\n", }, StringSplitOptions.RemoveEmptyEntries);

		if (0 < locationss.Length)
		{
			res_group[] groups = new res_group[locationss.Length];

			for (int index = 0; index < locationss.Length; ++index)
				groups[index] = new res_group(locationss[index] + "\\");

			thread = new Thread(new ParameterizedThreadStart(generate_xml));
			reserve = thread;

			thread.Start(groups);
		}
	}

	private void generate_xml(object parameter)
	{
		res_root res;
		res_group[] groups = parameter as res_group[];

		Invoke(new Action<int>(delegate(int o)
		{
			locations.Enabled = false;

			archive.Style = ProgressBarStyle.Marquee;
			total.Style = ProgressBarStyle.Marquee;

			button.Text = "x";
		}), 0);

		res = new res_root();

		foreach (res_group group in groups)
		{
			if (!res.characters.ContainsKey(group.location))
				res.characters.Add(group.location, new res_character(group.location));
			if (!res.items.ContainsKey(group.location))
				res.items.Add(group.location, new res_item(group.location));
		}

		Invoke(new Action<int>(delegate(int o)
		{
			archive.Style = ProgressBarStyle.Blocks;
			total.Style = ProgressBarStyle.Blocks;

			total.Maximum = groups.Length;
		}), 0);

		Directory.CreateDirectory(thumbnails);

		foreach (res_group group in groups)
			if (null != thread)
			{
				res_character character;
				res_item item;
				res_commodity commodities;
				wzproperty identities;

				Invoke(new Action<int>(delegate(int o)
				{
					++total.Value;

					archive.Value = 0;

					archive.Maximum = compute_packages_count(group.characters);
					archive.Maximum = archive.Maximum + compute_packages_count(group.items["Install"]);
					archive.Maximum = archive.Maximum + compute_packages_count(group.items["Pet"]);
				}), 0);

				character = res.characters[group.location];
				item = res.items[group.location];
				commodities = new res_commodity(group.commodities);
				identities = group.identities["Eqp.img"].root["", "Eqp"];
				packages = group.characters;

				while (null != packages.host)
					packages = packages.host;

				enumerate_characters(group.characters, character, res);
				enumerate_accessory(group.characters["Accessory"], identities["Accessory"], commodities, character.accessory, res);
				enumerate_cap(group.characters["Cap"], identities["Cap"], commodities, character.cap, res);
				enumerate_cape(group.characters["Cape"], identities["Cape"], commodities, character.cape, res);
				enumerate_coat(group.characters["Coat"], identities["Coat"], commodities, character.coat, res);
				enumerate_face(group.characters["Face"], identities["Face"], character.face, res);
				enumerate_glove(group.characters["Glove"], identities["Glove"], commodities, character.glove, res);
				enumerate_hair(group.characters["Hair"], identities["Hair"], character.hair, res);
				enumerate_longcoat(group.characters["Longcoat"], identities["Longcoat"], commodities, character.longcoat, res);
				enumerate_pants(group.characters["Pants"], identities["Pants"], commodities, character.pants, res);
				enumerate_petequip(group.characters["PetEquip"], identities["PetEquip"], commodities, character.petequip, res);
				enumerate_shield(group.characters["Shield"], identities["Shield"], commodities, character.shield, res);
				enumerate_shoes(group.characters["Shoes"], identities["Shoes"], commodities, character.shoes, res);
				enumerate_tamingmob(group.characters["TamingMob"], identities["Taming"], commodities, character.tamingmob, res);
				enumerate_weapon(group.characters["Weapon"], identities["Weapon"], commodities, character.weapon, res);

				enumerate_install(group.items["Install"], group.identities["Ins.img"].root[""], item.install, res);
				enumerate_pet(group.items["Pet"], group.identities["Pet.img"].root[""], commodities, item.pet, res);
			}
			else
			{
				break;
			}

		res.save();

		foreach (res_group group in groups)
			group.dispose();

		Invoke(new Action<int>(delegate(int o)
		{
			locations.Enabled = true;

			archive.Value = 0;
			total.Value = 0;

			button.Text = "o";
		}), 0);
	}

	private int compute_packages_count(wzpackage host)
	{
		int count = 0;

		foreach (wzpackage package in host)
		{
			if (0 < package.count)
				count = count + compute_packages_count(package);

			if (2 == package.type || 4 == package.type)
				++count;
		}

		return count;
	}

	private void enumerate_characters(wzpackage host, res_character character, res_root res)
	{
		string location = thumbnails + "character\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0001"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "", identity])
				{
					character.Add(identity, new res_object(
						"0001",
						identity,
						"",
						"0",
						gender_neutral,
						cash_negative,
						job_neutral,
						"0000" + identity.Substring(4)));

					generate_image(location, identity, package.root[""]["front", "head"]);
				}

				Invoke(new Action<int>(delegate(int o)
				{
					++archive.Value;
				}), 0);
			}
		}
	}

	private void enumerate_accessory(wzpackage host, wzproperty identities, res_commodity commodities, res_objects accessory, res_root res)
	{
		string location = thumbnails + "character\\accessory\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0101") || package.identity.StartsWith("0102") || package.identity.StartsWith("0103"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "accessory", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					accessory.Add(identity, new res_object(
						identity.Substring(0, 4),
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_cap(wzpackage host, wzproperty identities, res_commodity commodities, res_objects cap, res_root res)
	{
		string location = thumbnails + "character\\cap\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0100"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "cap", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					cap.Add(identity, new res_object(
						"0100",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_cape(wzpackage host, wzproperty identities, res_commodity commodities, res_objects cape, res_root res)
	{
		string location = thumbnails + "character\\cape\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0110"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "cape", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					cape.Add(identity, new res_object(
						"0110",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_coat(wzpackage host, wzproperty identities, res_commodity commodities, res_objects coat, res_root res)
	{
		string location = thumbnails + "character\\coat\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0104"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "coat", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					coat.Add(identity, new res_object(
						"0104",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_face(wzpackage host, wzproperty identities, res_objects face, res_root res)
	{
		string location = thumbnails + "character\\face\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in generate_list(host, "0002", 5))
		{
			if (null == thread)
				break;

			string identity = package.identity.Substring(0, 8);

			if (!res["character", "face", identity])
			{
				face.Add(identity, new res_object(
					"0002",
					identity,
					query_name(int.Parse(identity).ToString(), identities),
					"0",
					'1' == package.identity[4] || '4' == package.identity[4] ? "1" : "0",
					cash_negative,
					job_neutral,
					""));

				generate_image(location, identity, package.root[""]["default", "face"]);
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_glove(wzpackage host, wzproperty identities, res_commodity commodities, res_objects glove, res_root res)
	{
		string location = thumbnails + "character\\glove\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0108"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "glove", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					glove.Add(identity, new res_object(
						"0108",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_hair(wzpackage host, wzproperty identities, res_objects hair, res_root res)
	{
		string location = thumbnails + "character\\hair\\";

		Directory.CreateDirectory(location);

		foreach (string type in new[] { "0003", "0004" })
			foreach (wzpackage package in generate_list(host, type, 7))
			{
				if (null == thread)
					break;

				string identity = package.identity.Substring(0, 8);
				wzproperty property = package.root[""];

				if (!res["character", "hair", identity])
				{
					hair.Add(identity, new res_object(
						type,
						identity,
						query_name(int.Parse(identity).ToString(), identities),
						"0",
						'1' == package.identity[4] || '2' == package.identity[4] || '4' == package.identity[4] || '7' == package.identity[4] ? "1" : "0",
						cash_negative,
						job_neutral,
						""));

					if (!generate_image(location, identity, property["default", "hairOverHead"]))
						generate_image(location, identity, property["default", "hair"]);
				}

				Invoke(new Action<int>(delegate (int o)
				{
					++archive.Value;
				}), 0);
			}
	}

	private void enumerate_longcoat(wzpackage host, wzproperty identities, res_commodity commodities, res_objects longcoat, res_root res)
	{
		string location = thumbnails + "character\\longcoat\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0105"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "longcoat", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					longcoat.Add(identity, new res_object(
						"0105",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_pants(wzpackage host, wzproperty identities, res_commodity commodities, res_objects pants, res_root res)
	{
		string location = thumbnails + "character\\pants\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0106"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "pants", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					pants.Add(identity, new res_object(
						"0106",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_petequip(wzpackage host, wzproperty identities, res_commodity commodities, res_objects petequip, res_root res)
	{
		string location = thumbnails + "character\\petequip\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0180"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "petequip", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty property = package.root[""];
					wzproperty info = property["info"];
					wzproperty commodity = commodities[real];
					string etc = "";

					foreach (string pet in property.identities)
						if ("info" != pet)
							etc = etc + "," + pet;

					petequip.Add(identity, new res_object(
						"0180",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						job_neutral,
						0 < etc.Length ? etc.Remove(0, 1) : etc));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_shield(wzpackage host, wzproperty identities, res_commodity commodities, res_objects shield, res_root res)
	{
		string location = thumbnails + "character\\shield\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0109"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "shield", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					shield.Add(identity, new res_object(
						"0109",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_shoes(wzpackage host, wzproperty identities, res_commodity commodities, res_objects shoes, res_root res)
	{
		string location = thumbnails + "character\\shoes\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0107"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "shoes", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					shoes.Add(identity, new res_object(
						"0107",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_tamingmob(wzpackage host, wzproperty identities, res_commodity commodities, res_objects tamingmob, res_root res)
	{
		string location = thumbnails + "character\\tamingmob\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0191"))
			{
				foreach (string mob in package.root[""].identities)
					if ("info" != mob)
					{
						string identity = int.Parse(mob).ToString("d8");

						if (!res["character", "tamingmob", identity])
						{
							wzpackage taming = host[identity + ".img"];

							if (null != taming)
							{
								wzproperty info = taming.root["", "info"];
								wzproperty commodity = commodities[mob];

								tamingmob.Add(identity, new res_object(
									"019",
									identity,
									query_name(mob, identities),
									query_level(info, commodity),
									query_gender(commodity),
									query_cash(info),
									query_job(info),
									package.identity.Substring(0, 8)));

								generate_image(location, identity, info["iconRaw"]);
							}
						}
					}
			}
			else if (package.identity.StartsWith("0193") || package.identity.StartsWith("0198") || package.identity.StartsWith("0199"))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "tamingmob", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					tamingmob.Add(identity, new res_object(
						"019",
						identity,
						query_name(real, identities),
						query_level(info, commodity),
						query_gender(commodity),
						query_cash(info),
						query_job(info),
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_weapon(wzpackage host, wzproperty identities, res_commodity commodities, res_objects weapon, res_root res)
	{
		string location = thumbnails + "character\\weapon\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (!package.identity.StartsWith("0160") && (package.identity.StartsWith("0135") ? (package.identity.StartsWith("013526") || package.identity.StartsWith("013530")) : true))
			{
				string identity = package.identity.Substring(0, 8);

				if (!res["character", "weapon", identity])
				{
                    if (int.TryParse(identity, out int r))
                    {
                        string real = r.ToString();
                        wzproperty property = package.root[""];
                        wzproperty info = property["info"];
                        wzproperty commodity = commodities[real];
                        string etc = "";

                        if ('7' == package.identity[2])
                        {
                            foreach (string type in property.identities)
                                if ("info" != type)
                                    etc = etc + "," + type;

                            etc = etc.Remove(0, 1);
                        }

                        weapon.Add(identity, new res_object(
                            identity.Substring(0, identity.StartsWith("0135") ? 6 : 4),
                            identity,
                            query_name(real, identities),
                            query_level(info, commodity),
                            query_gender(commodity),
                            query_cash(info),
                            query_job(info),
                            etc));

                        generate_image(location, identity, info["iconRaw"]);
                    }
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_install(wzpackage host, wzproperty identities, res_objects install, res_root res)
	{
		string location = thumbnails + "item\\install\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("0301") || package.identity.StartsWith("0302"))
			{
				wzproperty property = package.root[""];

				foreach (wzproperty chair in property)
				{
					if (!res["item", "install", chair.identity])
					{
						wzproperty info = chair["info"];

						install.Add(chair.identity, new res_object(
							"030",
							chair.identity,
							query_name(int.Parse(chair.identity).ToString(), identities),
							query_level(info, null),
							gender_neutral,
							cash_negative,
							job_neutral,
							""));

						generate_image(location, chair.identity, info["iconRaw"]);
					}
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private void enumerate_pet(wzpackage host, wzproperty identities, res_commodity commodities, res_objects pet, res_root res)
	{
		string location = thumbnails + "item\\pet\\";

		Directory.CreateDirectory(location);

		foreach (wzpackage package in host)
		{
			if (null == thread)
				break;

			if (package.identity.StartsWith("500"))
			{
				string identity = package.identity.Substring(0, 7);

				if (!res["item", "pet", identity])
				{
					string real = int.Parse(identity).ToString();
					wzproperty info = package.root["", "info"];
					wzproperty commodity = commodities[real];

					pet.Add(identity, new res_object(
						"500",
						identity,
						query_name(real, identities),
						"0",
						query_gender(commodity),
						query_cash(info),
						job_neutral,
						""));

					generate_image(location, identity, info["iconRaw"]);
				}
			}

			Invoke(new Action<int>(delegate(int o)
			{
				++archive.Value;
			}), 0);
		}
	}

	private string query_name(string identity, wzproperty identities)
	{
		return identities.query("", identity, "name");
	}

	private string query_level(wzproperty info, wzproperty commodity)
	{
		if (null != info)
			if (info.query(out info, "reqLevel"))
				return info.query("0");

		return null == commodity ? "0" : commodity.query("0", "ReqLEV");
	}

	private string query_gender(wzproperty commodity)
	{
		return null == commodity ? gender_neutral : commodity.query(gender_neutral, "Gender");
	}

	private string query_cash(wzproperty info)
	{
		return null == info ? cash_negative : info.query(cash_negative, "cash");
	}

	private string query_job(wzproperty info)
	{
		return null == info ? job_neutral : info.query(job_neutral, "reqJob");
	}

	private bool generate_image(string prefix, string identity, wzproperty property)
	{
		if (null != property)
		{
			wzcanvas canvas = inspect_canvas1(property);

			if (null != canvas)
			{
				canvas.image.Save(prefix + identity + ".png");

				return true;
			}
		}

		return false;
	}

	private List<wzpackage> generate_list(wzpackage host, string preffix, int index)
	{
		List<wzpackage> packages = new List<wzpackage>();

		foreach (wzpackage package in host)
			if (package.identity.StartsWith(preffix))
				if (generate_list(packages, package, index))
					Invoke(new Action<int>(delegate(int o)
					{
						++archive.Value;
					}), 0);
				else
					packages.Add(package);

		return packages;
	}

	private bool generate_list(List<wzpackage> packages, wzpackage target, int index)
	{
		foreach (wzpackage package in packages)
			if (package.identity.Remove(index, 1) == target.identity.Remove(index, 1))
				return true;

		return false;
	}

	private wzproperty inspect_canvas(wzproperty property)
	{
		if (4 == property.type)
			return property;
		if (5 == property.type)
			return inspect_canvas((property.data as wzuol).target);

		return null;
	}

	private wzcanvas inspect_canvas1(wzproperty property)
	{
		property = inspect_canvas(property);

		if (null != property)
		{
			wzcanvas source;
			string link = property.query(null as string, "source") ?? property.query(null as string, "_outlink") ?? property.query(null as string, "_inlink");

			if (null == link)
			{
				source = property.query<wzcanvas>(null);
			}
			else
			{
				int index = link.IndexOf(".img/");

				if (0 <= index)
					source = wzproperty.query<wzcanvas>(packages.query(link.Substring(0, index + 4).Split('/')), null, link.Substring(index + 5).Split('/'));
				else
					source = property.root.query<wzcanvas>(null, link.Split('/'));
			}

			return source;
		}

		return null;
	}

	private class res_commodity : Dictionary<string, wzproperty>
	{
		internal new wzproperty this[string key]
		{
			get
			{
				wzproperty property;

				return TryGetValue(key, out property) ? property : null;
			}
		}

		internal res_commodity(wzpackage package)
		{
			foreach (wzproperty properties in package.root[""])
			{
				string identity = properties.query("", "ItemId");

				if (!ContainsKey(identity))
					Add(identity, properties);
			}
		}
	}

	private class res_group
	{
		private wzarchives archives;

		private object[] properties;

		internal wzpackage characters
		{
			get
			{
				return properties[0] as wzpackage;
			}
		}

		internal wzpackage items
		{
			get
			{
				return properties[1] as wzpackage;
			}
		}

		internal wzpackage identities
		{
			get
			{
				return properties[2] as wzpackage;
			}
		}

		internal wzpackage commodities
		{
			get
			{
				return properties[3] as wzpackage;
			}
		}

		internal string location
		{
			get
			{
				return properties[4] as string;
			}
		}

		internal res_group(string location)
		{
			archives = new wzarchives(location + "base.wz");

			properties = new object[]
			{
				archives.root["Character"],
				archives.root["Item"],
				archives.root["String"],
				archives.root["Etc", "Commodity.img"],
				location,
			};
		}

		internal void dispose()
		{
			archives.dispose();
		}
	}

	private class res_root
	{
		private object[] properties;

		internal Dictionary<string, res_character> characters
		{
			get
			{
				return properties[0] as Dictionary<string, res_character>;
			}
		}

		internal Dictionary<string, res_item> items
		{
			get
			{
				return properties[1] as Dictionary<string, res_item>;
			}
		}

		internal bool this[string genus, string species, string identity]
		{
			get
			{
				if ("character" == genus)
				{
					foreach (res_character character in characters.Values)
						if (character[species, identity])
							return true;
				}
				else if ("item" == genus)
				{
					foreach (res_item item in items.Values)
						if (item[species, identity])
							return true;
				}

				return false;
			}
		}

		private string components
		{
			get
			{
				return momiji_host.assembly + "\\" + "momiji" + ".components";
			}
		}

		internal res_root()
		{
			properties = new object[]
			{
				new Dictionary<string, res_character>(),
				new Dictionary<string, res_item>(),
			};

			if (File.Exists(components))
			{
				XmlDocument document = new XmlDocument();

				document.Load(components);

				foreach (XmlNode node in document["momiji"])
					if ("character" == node.Name)
						characters.Add(node.Attributes["location"].Value, new res_character(node));
					else if ("item" == node.Name)
						items.Add(node.Attributes["location"].Value, new res_item(node));
			}
		}

		internal void save()
		{
			XmlDocument document = new XmlDocument();
			XmlElement element = document.CreateElement("momiji");

			foreach (res_character character in characters.Values)
				character.save(element, document);

			foreach (res_item item in items.Values)
				item.save(element, document);

			document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			document.AppendChild(element);

			document.Save(components);
		}
	}

	private class res_character : res_objects
	{
		private readonly static List<string> specieses = new List<string>()
		{
			"accessory", "cap", "cape", "coat", "face", "glove", "hair",
			"longcoat", "pants", "petequip", "shield", "shoes", "tamingmob", "weapon",
		};

		private object[] properties;

		internal res_objects accessory
		{
			get
			{
				return properties[0] as res_objects;
			}
			set
			{
				properties[0] = value;
			}
		}

		internal res_objects cap
		{
			get
			{
				return properties[1] as res_objects;
			}
			set
			{
				properties[1] = value;
			}
		}

		internal res_objects cape
		{
			get
			{
				return properties[2] as res_objects;
			}
			set
			{
				properties[2] = value;
			}
		}

		internal res_objects coat
		{
			get
			{
				return properties[3] as res_objects;
			}
			set
			{
				properties[3] = value;
			}
		}

		internal res_objects face
		{
			get
			{
				return properties[4] as res_objects;
			}
			set
			{
				properties[4] = value;
			}
		}

		internal res_objects glove
		{
			get
			{
				return properties[5] as res_objects;
			}
			set
			{
				properties[5] = value;
			}
		}

		internal res_objects hair
		{
			get
			{
				return properties[6] as res_objects;
			}
			set
			{
				properties[6] = value;
			}
		}

		internal res_objects longcoat
		{
			get
			{
				return properties[7] as res_objects;
			}
			set
			{
				properties[7] = value;
			}
		}

		internal res_objects pants
		{
			get
			{
				return properties[8] as res_objects;
			}
			set
			{
				properties[8] = value;
			}
		}

		internal res_objects petequip
		{
			get
			{
				return properties[9] as res_objects;
			}
			set
			{
				properties[9] = value;
			}
		}

		internal res_objects shield
		{
			get
			{
				return properties[10] as res_objects;
			}
			set
			{
				properties[10] = value;
			}
		}

		internal res_objects shoes
		{
			get
			{
				return properties[11] as res_objects;
			}
			set
			{
				properties[11] = value;
			}
		}

		internal res_objects tamingmob
		{
			get
			{
				return properties[12] as res_objects;
			}
			set
			{
				properties[12] = value;
			}
		}

		internal res_objects weapon
		{
			get
			{
				return properties[13] as res_objects;
			}
			set
			{
				properties[13] = value;
			}
		}

		internal string location
		{
			get
			{
				return properties[14] as string;
			}
			set
			{
				properties[14] = value;
			}
		}

		internal bool this[string species, string identity]
		{
			get
			{
				if (0 < species.Length)
				{
					int index = -1;
					res_objects objects = properties[specieses.IndexOf(species)] as res_objects;

					if ("face" == species)
						index = 5;
					else if ("hair" == species)
						index = 7;
					else
						return objects.ContainsKey(identity);

					identity = identity.Remove(index, 1);

					foreach (string source in objects.Keys)
						if (source.Remove(index, 1) == identity)
							return true;

					return false;
				}
				else
				{
					return ContainsKey(identity);
				}
			}
		}

		internal res_character(string location)
		{
			properties = new object[]
			{
				new res_objects("accessory"),
				new res_objects("cap"),
				new res_objects("cape"),
				new res_objects("coat"),
				new res_objects("face"),
				new res_objects("glove"),
				new res_objects("hair"),
				new res_objects("longcoat"),
				new res_objects("pants"),
				new res_objects("petequip"),
				new res_objects("shield"),
				new res_objects("shoes"),
				new res_objects("tamingmob"),
				new res_objects("weapon"),
				location,
			};
		}

		internal res_character(XmlNode host)
		{
			properties = new object[15];

			if (null != host)
			{
				int index = 0;

				location = host.Attributes["location"].Value;

				foreach (XmlNode node in host)
					if ("object" == node.Name)
						Add(node.Attributes["id"].Value, new res_object(node));
					else
						properties[index++] = new res_objects(node);
			}
		}

		internal new void save(XmlNode host, XmlDocument document)
		{
			XmlElement element = document.CreateElement("character");

			element.Attributes.Append(generate_attribute("location", location, document));

			save_items(element, document);

			for (int index = 0; index < properties.Length - 1; ++index)
				(properties[index] as res_objects).save(element, document);

			host.AppendChild(element);
		}
	}

	private class res_item
	{
		private readonly static List<string> specieses = new List<string>() { "install", "pet", };

		private object[] properties;

		internal res_objects install
		{
			get
			{
				return properties[0] as res_objects;
			}
			set
			{
				properties[0] = value;
			}
		}

		internal res_objects pet
		{
			get
			{
				return properties[1] as res_objects;
			}
			set
			{
				properties[1] = value;
			}
		}

		internal string location
		{
			get
			{
				return properties[2] as string;
			}
			set
			{
				properties[2] = value;
			}
		}

		internal bool this[string species, string identity]
		{
			get
			{
				return (properties[specieses.IndexOf(species)] as res_objects).ContainsKey(identity);
			}
		}

		internal res_item(string location)
		{
			properties = new object[]
			{
				new res_objects("install"),
				new res_objects("pet"),
				location,
			};
		}

		internal res_item(XmlNode host)
		{
			properties = new object[3];

			if (null != host)
			{
				location = host.Attributes["location"].Value;

				foreach (XmlNode node in host)
					if ("install" == node.Name)
						install = new res_objects(node);
					else if ("pet" == node.Name)
						pet = new res_objects(node);
			}
		}

		internal void save(XmlNode host, XmlDocument document)
		{
			XmlElement element = document.CreateElement("item");

			element.Attributes.Append(generate_attribute("location", location, document));

			for (int index = 0; index < properties.Length - 1; ++index)
				(properties[index] as res_objects).save(element, document);

			host.AppendChild(element);
		}
	}

	private class res_objects : SortedDictionary<string, res_object>
	{
		private string[] properties;

		internal string name
		{
			get
			{
				return properties[0];
			}
			set
			{
				properties[0] = value;
			}
		}

		internal res_objects()
		{
			properties = new string[1];
		}

		internal res_objects(string name)
		{
			properties = new string[]
			{
				name,
			};
		}

		internal res_objects(XmlNode host)
		{
			properties = new string[]
			{
				host.Name,
			};

			foreach (XmlNode node in host)
				Add(node.Attributes["id"].Value, new res_object(node));
		}

		internal void save(XmlNode host, XmlDocument document)
		{
			XmlElement element = document.CreateElement(name);

			save_items(element, document);

			host.AppendChild(element);
		}

		protected void save_items(XmlNode host, XmlDocument document)
		{
			foreach (res_object item in this.Values)
				item.save(host, document);
		}
	}

	private class res_object
	{
		private string[] properties;

		internal string type
		{
			get
			{
				return properties[0];
			}
		}

		internal string identity
		{
			get
			{
				return properties[1];
			}
		}

		internal string name
		{
			get
			{
				return properties[2];
			}
		}

		internal string level
		{
			get
			{
				return properties[3];
			}
		}

		internal string gender
		{
			get
			{
				return properties[4];
			}
		}

		internal string cash
		{
			get
			{
				return properties[5];
			}
		}

		internal string job
		{
			get
			{
				return properties[6];
			}
		}

		internal string etc
		{
			get
			{
				return properties[7];
			}
		}

		internal res_object(string type, string identity, string name, string level, string gender, string cash, string job, string etc)
		{
			properties = new string[]
			{
				type,
				identity,
				name,
				level,
				gender,
				cash,
				job,
				etc,
			};
		}

		internal res_object(XmlNode node)
		{
			properties = new string[]
			{
				node.Attributes["type"].Value,
				node.Attributes["id"].Value,
				node.Attributes["name"].Value,
				node.Attributes["level"].Value,
				node.Attributes["gender"].Value,
				node.Attributes["cash"].Value,
				node.Attributes["job"].Value,
				node.Attributes["etc"].Value,
			};
		}

		internal void save(XmlNode host, XmlDocument document)
		{
			XmlElement element = document.CreateElement("object");

			element.Attributes.Append(generate_attribute("type", type, document));
			element.Attributes.Append(generate_attribute("id", identity, document));
			element.Attributes.Append(generate_attribute("name", name, document));
			element.Attributes.Append(generate_attribute("level", level, document));
			element.Attributes.Append(generate_attribute("gender", gender, document));
			element.Attributes.Append(generate_attribute("cash", cash, document));
			element.Attributes.Append(generate_attribute("job", job, document));
			element.Attributes.Append(generate_attribute("etc", etc, document));

			host.AppendChild(element);
		}
	}
}
