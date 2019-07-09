using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

internal class scenes_host : momiji_owned_base
{
	private ToolStripMenuItem item_pet;
	private ToolStripMenuItem item_image;
	private ToolStripSplitButton item_character;
	private ToolStripButton item_delete;
	private ToolStripButton item_duplicate;
	private ToolStripButton item_code;
	private ToolStripSeparator item_separator_code_flip;
	private ToolStripButton item_flip;
	private ToolStripSeparator item_separator_flip_save;
	private ToolStripMenuItem item_save_all;
	private ToolStripSplitButton item_save;
	
	private momiji_toolstrip toolstrip;
	private momiji_tabcontrol tabcontrol;
	private momiji_listview listview;

	private List<string> zorders;

	private int ordinal;

	private momiji_host momiji;
	private components_host components;
	private actions_host actions;

	internal string type
	{
		get
		{
			return null == tabcontrol.SelectedTab ? null : (tabcontrol.SelectedTab.Tag as scenes_container).type;
		}
	}

	internal scenes_component this[int index]
	{
		get
		{
			return null == tabcontrol.SelectedTab ? null : (tabcontrol.SelectedTab.Tag as scenes_container)[index];
		}
	}

	internal scenes_host(momiji_host host)
	{
		item_pet = new ToolStripMenuItem(null, query_image("toolstrip_pet"), toolstrip_add_pet_click);
		item_image = new ToolStripMenuItem(null, query_image("toolstrip_image"), toolstrip_add_image_click);
		item_character = new ToolStripSplitButton(null, query_image("toolstrip_character"), item_pet, item_image);
		item_delete = new ToolStripButton(null, query_image("toolstrip_delete"), toolstrip_delete_click);
		item_duplicate = new ToolStripButton(null, query_image("toolstrip_duplicate"), toolstrip_duplicate_click);
		item_code = new ToolStripButton(null, query_image("toolstrip_code"), toolstrip_code_click);
		item_separator_code_flip = new ToolStripSeparator();
		item_flip = new ToolStripButton(null, query_image("toolstrip_flip"), toolstrip_flip_horizontal_click);
		item_separator_flip_save = new ToolStripSeparator();
		item_save_all = new ToolStripMenuItem(null, query_image("toolstrip_save_all"), toolstrip_save_all_click);
		item_save = new ToolStripSplitButton(null, query_image("toolstrip_save"), item_save_all);

		toolstrip = new momiji_toolstrip();
		tabcontrol = new momiji_tabcontrol();
		listview = new momiji_listview();

		tabcontrol.Dock = DockStyle.Fill;
		tabcontrol.Parent = this;

		tabcontrol.SelectedIndexChanged += tabcontrol_selected_index_changed;
		tabcontrol.KeyDown += tabcontrol_key_down;

		listview.FullRowSelect = true;
		listview.HideSelection = false;
		listview.ShowItemToolTips = true;
		listview.Visible = false;
		listview.Dock = DockStyle.Fill;
		listview.HeaderStyle = ColumnHeaderStyle.None;
		listview.View = View.Tile;
		listview.LargeImageList = new ImageList();
		listview.TileSize = new Size(150, 40);

		listview.Columns.Add("");
		listview.Columns.Add("");

		listview.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;
		listview.LargeImageList.ImageSize = new Size(40, 40);

		listview.KeyDown += listview_key_down;

		toolstrip.Parent = this;

		toolstrip.Items.AddRange(new ToolStripItem[]
		{
			item_character,
			item_delete,
			item_duplicate,
			item_code,
			item_separator_code_flip,
			item_flip,
			item_separator_flip_save,
			item_save,
		});

		item_delete.Enabled = false;
		item_duplicate.Enabled = false;
		item_flip.Enabled = false;
		item_save.Enabled = false;

		item_character.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_duplicate.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_code.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_flip.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_save.DisplayStyle = ToolStripItemDisplayStyle.Image;

		item_image.Visible = false;

		item_character.ButtonClick += toolstrip_add_character_click;
		item_save.ButtonClick += toolstrip_save_click;

		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.Manual;
		Size = new Size(200, 400);

		FormClosing += host.owned_form_closing;

		momiji = host;

		load_zorders();

		ordinal = 0;

		if (momiji_preferences.general_explorer_style)
			enable_explorer_style(listview, true);

		switch_language();
	}

	internal override void switch_language()
	{
		item_character.Text = momiji_languages.scenes_toolstrip_character;
		item_pet.Text = momiji_languages.scenes_toolstrip_pet;
		item_image.Text = momiji_languages.scenes_toolstrip_image;
		item_delete.Text = momiji_languages.scenes_toolstrip_delete;
		item_duplicate.Text = momiji_languages.scenes_toolstrip_duplicate;
		item_code.Text = momiji_languages.scenes_toolstrip_code;
		item_flip.Text = momiji_languages.scenes_toolstrip_flip;
		item_save.Text = momiji_languages.scenes_toolstrip_save;
		item_save_all.Text = momiji_languages.scenes_toolstrip_save_all;

		Text = momiji_languages.scenes_caption;

		switch_font();
	}

	internal override void update_preferences()
	{
		if (momiji_preferences.tgeneral_explorer_style != momiji_preferences.general_explorer_style)
		{
			enable_explorer_style(listview, momiji_preferences.general_explorer_style);

			listview.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}
	}

	internal void initial(components_host components, actions_host actions)
	{
		this.components = components;
		this.actions = actions;
	}

	internal void show(momiji_host host)
	{
		Point client = host.PointToScreen(Point.Empty);

		Location = new Point(client.X + host.ClientSize.Width - Width, client.Y);
		position = new Point(Left - host.Left, Top - host.Top);

		Show(host);
	}

	private void load_zorders()
	{
		wzarchives archives = new wzarchives(momiji_host.assembly + "\\" + "base.wz");
		wzpackage package = archives.root;

		zorders = (null == package) ? new List<string>() : new List<string>(package["zmap.img"].root[""].identities);

		zorders.Reverse();
	}

	internal void scene_selected(scenes_container container)
	{
		foreach (TabPage tabpage in tabcontrol.TabPages)
			if (container == tabpage.Tag)
			{
				tabcontrol.SelectedTab = tabpage;

				Refresh();

				break;
			}
	}

	internal void add_scene(string type)
	{
		components_object component;

		if ("character" == type)
			component = components.character_essential;
		else if ("pet" == type)
			component = components.pet_essential;
		else
			return;

		if (null != component)
		{
			scenes_scene_base scene;

			if ("500" == component.type)
			{
				scene = new scenes_pet(component);
			}
			else
			{
				wzpackage package;

				if (null == component.package || 1 > component.etc.Count)
					return;

				if (!component.package.host.query(out package, component.etc[0] + ".img"))
					if (!component.package.host.query(out package, "00002000.img"))
						return;

				scene = new scenes_character(component, package);
			}

			add_scene(new scenes_container(type, scene, zorders));
		}
	}

	private void add_scene(scenes_container container)
	{
		if (null != container)
		{
			TabPage page = new TabPage(container.type[0].ToString() + ordinal++);

			container.update_listview(listview);

			page.Tag = container;

			tabcontrol.TabPages.Add(page);

			tabcontrol.selected_index = -1;
			tabcontrol.SelectedTab = page;

			components.update_focus();

			item_delete.Enabled = true;
			item_duplicate.Enabled = true;
			item_flip.Enabled = true;
			item_save.Enabled = true;
		}
	}

	internal void update_scene(components_object component, components_species species, int index, components_sources sources)
	{
        Console.WriteLine(component.type);
		if (null != component && null != tabcontrol.SelectedTab)
			if (null != component.package)
			{
				wzpackage package;
				scenes_container container = tabcontrol.SelectedTab.Tag as scenes_container;

				if ("0001" == component.type)
				{
					if (1 > component.etc.Count)
						return;

					if (!component.package.host.query(out package, component.etc[0] + ".img"))
						if (!component.package.host.query(out package, "00002000.img"))
							return;

					if (!container.update_component(component, package))
						return;
				}
				else if ("0002" == component.type || "0003" == component.type || "0004" == component.type)
				{
					string identity = component.id.Remove(species.etc, 1).Insert(species.etc, index.ToString());

					if ((tabcontrol.SelectedTab.Tag as scenes_container).scene[identity])
						return;

					if (!component.package.host.query(out package, identity + ".img"))
						package = component.package;

					if (!container.update_component(component, package))
						return;
				}
				else if ("019" == component.type)
				{
					if (0 < component.etc.Count)
						component.package.host.query(out package, component.etc[0] + ".img");
					else
						package = null;

					if (!container.update_component(component, package))
						return;
				}
				else
				{
					if (!container.update_component(component, species))
						return;

					if ("500" == component.type && null != this[1])
						container.update_component(this[1].component, components.genuses["pet"]["0180"]);
				}

				container.update_listview(listview);

				actions.update_actions();
			}
	}

	private void delete_item()
	{
		if (null != tabcontrol.SelectedTab)
		{
			int index;
			TabPage page;
			scenes_container container = tabcontrol.SelectedTab.Tag as scenes_container;

			if (0 < listview.SelectedItems.Count)
			{
				foreach (ListViewItem item in listview.SelectedItems)
				{
					if (container.delete_component(item.Tag as scenes_component))
					{
						container.update_listview(listview);

						continue;
					}

					goto _delete_scene;
				}

				actions.current = container;

				return;
			}

		_delete_scene:
			index = tabcontrol.SelectedIndex;
			page = tabcontrol.SelectedTab;

			tabcontrol.selected_index = -1;

			tabcontrol.TabPages.Remove(page);

			page.Dispose();

			if (0 < tabcontrol.TabPages.Count)
			{
				tabcontrol.selected_index = tabcontrol.TabPages.Count > index ? index : tabcontrol.TabPages.Count - 1;
			}
			else
			{
				item_delete.Enabled = false;
				item_duplicate.Enabled = false;
				item_flip.Enabled = false;
				item_save.Enabled = false;
			}

			components.update_focus();

			if (1 > tabcontrol.TabPages.Count)
				actions.current = null;

			momiji.delete_animation(index);
			actions.delete_animation(index);
		}
	}

	private void analyze_code(string code, List<string> z)
	{
		add_scene(scenes_container.analyze_code(code, components.genuses, z));
	}

	private void toolstrip_add_character_click(object o, EventArgs e)
	{
		add_scene("character");
	}

	private void toolstrip_add_pet_click(object o, EventArgs e)
	{
		add_scene("pet");
	}

	private void toolstrip_add_image_click(object o, EventArgs e)
	{
	}

	private void toolstrip_delete_click(object o, EventArgs e)
	{
		delete_item();
	}

	private void toolstrip_duplicate_click(object o, EventArgs e)
	{
		if (null != tabcontrol.SelectedTab)
			analyze_code((tabcontrol.SelectedTab.Tag as scenes_container).code, zorders);
	}

	private void toolstrip_code_click(object o, EventArgs e)
	{
		string code = new code(null == tabcontrol.SelectedTab ? "" : (tabcontrol.SelectedTab.Tag as scenes_container).code).show_dialog();

		if (1 < code.Length)
			foreach (string scene in code.Split(new char[] { ';', }, StringSplitOptions.RemoveEmptyEntries))
				analyze_code(scene, zorders);
	}

	private void toolstrip_flip_horizontal_click(object o, EventArgs e)
	{
		actions.flip_animation();
	}

	private void toolstrip_save_click(object o, EventArgs e)
	{
		actions.save_animation(true);
	}

	private void toolstrip_save_all_click(object o, EventArgs e)
	{
		actions.save_animation(false);
	}

	private void tabcontrol_selected_index_changed(object o, EventArgs e)
	{
		listview.Visible = false;
		listview.Parent = null;

		if (null != tabcontrol.SelectedTab)
		{
			scenes_container container = tabcontrol.SelectedTab.Tag as scenes_container;

			container.update_listview(listview);

			listview.Parent = tabcontrol.SelectedTab;
			listview.Visible = true;

			enable_explorer_style(listview, momiji_preferences.general_explorer_style);

			components.update_focus();

			actions.current = container;
		}
	}

	private void tabcontrol_key_down(object o, KeyEventArgs e)
	{
		if (null != tabcontrol.SelectedTab)
		{
			int delta, index, position;

			if (Keys.OemOpenBrackets == e.KeyCode)
				delta = -1;
			else if (Keys.OemCloseBrackets == e.KeyCode)
				delta = +1;
			else
				return;

			index = tabcontrol.SelectedIndex;
			position = index + delta;

			if (0 <= position && tabcontrol.TabPages.Count > position)
			{
				TabPage current = tabcontrol.SelectedTab;

				tabcontrol.TabPages.Remove(current);
				tabcontrol.TabPages.Insert(position, current);

				tabcontrol.SelectedTab = current;

				momiji.move_animation(index, delta);
				actions.move_animation(index, delta);
			}
		}
	}

	private void listview_key_down(object o, KeyEventArgs e)
	{
		if (0 < listview.SelectedItems.Count && Keys.Delete == e.KeyCode)
			delete_item();
	}
}

internal class scenes_container
{
	internal static scenes_container analyze_code(string code, components_genuses genuses, List<string> z)
	{
		components_genus genus;
		components_species species;
		components_object component;
		string[] entries = code.Replace("\r\n", "").Split(new char[] { ',', }, StringSplitOptions.RemoveEmptyEntries);

		if (1 < entries.Length)
			if ("c" == entries[0])
			{
				if (genuses.TryGetValue("host", out genus))
					if (genus.TryGetValue("0001", out species))
						for (int index = 1; index < entries.Length; ++index)
							if (species.objects.TryGetValue(entries[index], out component))
								return new scenes_container("character", scenes_character.analyze_code(entries, component, genuses), z);
			}
			else if ("p" == entries[0])
			{
				if (genuses.TryGetValue("pet", out genus))
					if (genus.TryGetValue("500", out species))
						for (int index = 1; index < entries.Length; ++index)
							if (species.objects.TryGetValue(entries[index], out component))
								return new scenes_container("pet", scenes_pet.analyze_code(entries, component, genuses), z);
			}

		return null;
	}

	private List<string> zorders;

	private object[] properties;

	internal string type
	{
		get
		{
			return properties[0] as string;
		}
	}

	internal scenes_scene_base scene
	{
		get
		{
			return properties[1] as scenes_scene_base;
		}
	}

	internal string code
	{
		get
		{
			return scene.code;
		}
	}

	internal List<string> actions
	{
		get
		{
			return scene.actions;
		}
	}

	internal List<string> emotions
	{
		get
		{
			return "character" == type ? (scene as scenes_character).emotions : null;
		}
	}

	internal scenes_component this[int index]
	{
		get
		{
			return scene[index];
		}
	}

	internal scenes_container(string type, scenes_scene_base scene, List<string> z)
	{
		zorders = z;
		properties = new object[]
		{
			type,
			scene,
		};
	}

	internal bool update_component(components_object component, object etc)
	{
		if ("500" == component.type || "0180" == component.type)
		{
			if ("pet" != type)
				return false;
		}
		else
		{
			if ("character" != type)
				return false;
		}

		return scene.update_component(component, etc);
	}

	internal void update_listview(ListView listview)
	{
		scene.update_listview(listview);
	}

	internal bool delete_component(scenes_component component)
	{
		return scene.delete_component(component);
	}

	internal momiji_animation generate_animation(string action, string emotion, bool cover, bool shadow, bool elf, string weapon)
	{
		return scene.generate_animation(action, emotion, cover, shadow, elf, weapon, zorders);
	}
}

internal class scenes_character : scenes_scene_base
{
	internal static scenes_character analyze_code(string[] entries, components_object component, components_genuses genuses)
	{
		wzpackage package;
		scenes_character scene;
		string essential;

		if (null == component.package || 1 > component.etc.Count)
			return null;

		if (!component.package.host.query(out package, component.etc[0] + ".img"))
			if (!component.package.host.query(out package, "00002000.img"))
				return null;

		scene = new scenes_character(component, package);
		essential = component.id;

		for (int id, index = 1; index < entries.Length; ++index)
			if (essential != entries[index])
			{
				string[] symbols = entries[index].Split(new char[] { '|', }, StringSplitOptions.RemoveEmptyEntries);

				if (int.TryParse(symbols[0], out id))
				{
					string type = (1 < symbols.Length) ? symbols[1] : "";

					foreach (components_genus genus in genuses.Values)
						if ("pet" != genus.type)
							foreach (components_species species in genus.Values)
							{
								component = species[symbols[0]];

								if (null != component)
									if ("0002" == species.type || "0003" == species.type || "0004" == species.type)
									{
										if (!component.package.host.query(out package, type + ".img"))
											package = component.package;

										scene.update_component(component, package);

										break;
									}
									else if ("019" == species.type)
									{
										if (0 < component.etc.Count)
											component.package.host.query(out package, component.etc[0] + ".img");

										scene.update_component(component, package);
									}
									else if ("0001" != species.type)
									{
										if ("0170" == component.type)
										{
											if (type != species.type)
												continue;
										}

										scene.update_component(component, species);

										break;
									}
							}
				}
			}

		return scene;
	}

	protected override int max
	{
		get
		{
			return 19;
		}
	}

	internal override string code
	{
		get
		{
			return generate_code("c");
		}
	}

	internal List<string> emotions
	{
		get
		{
			return null == properties[2] ? null : (properties[2] as List<string>);
		}
	}

	internal scenes_character(components_object component, wzpackage package)
		: base(component)
	{
		components[1] = new scenes_component(package.root[""]);

		update_actions(components[1].property, components[0].property);
	}

	internal override bool update_component(components_object component, object etc)
	{
		int index;
		string sign;
		wzproperty property;

		if ("0001" == component.type)
		{
			index = 0;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];

			components[1] = new scenes_component((etc as wzpackage).root[""]);
		}
		else if ("0002" == component.type)
		{
			wzpackage package = (etc as wzpackage);
			string id = package.identity.Substring(0, 8);

			index = 2;
			sign = component.id + "|" + id;

			if (!isequals(index, sign))
				return false;

			property = package.root[""];
			component = new components_object(id, component, package);

			update_emotions(property);
		}
		else if ("0003" == component.type || "0004" == component.type)
		{
			wzpackage package = (etc as wzpackage);
			string id = package.identity.Substring(0, 8);

			index = 3;
			sign = component.id + "|" + id;

			if (!isequals(index, sign))
				return false;

			property = package.root[""];
			component = new components_object(id, component, package);
		}
		else if ("0100" == component.type)
		{
			index = 4;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];
		}
		else if ("0101" == component.type)
		{
			index = 5;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];

			if (null == components[2])
				update_emotions(property);
		}
		else if ("0102" == component.type)
		{
			index = 6;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];
		}
		else if ("0103" == component.type)
		{
			index = 7;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];
		}
		else if ("0104" == component.type)
		{
			index = 8;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];

			components[9] = null;
		}
		else if ("0105" == component.type)
		{
			index = 9;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];

			components[8] = null;
			components[10] = null;
		}
		else if ("0106" == component.type)
		{
			index = 10;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];

			components[9] = null;
		}
		else if ("0107" == component.type)
		{
			index = 11;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];
		}
		else if ("0108" == component.type)
		{
			index = 12;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];
		}
		else if ("0109" == component.type)
		{
			index = 13;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];

			if (null != components[15])
				if (2 == components[15].etc)
					components[15] = null;

			if (null == components[15] && null == components[16] && null == components[18])
				update_actions(components[1].property, property);
		}
		else if ("0110" == component.type)
		{
			index = 14;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];
		}
		else if ("019" == component.type)
		{
			index = 16;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];

			components[17] = (null == etc) ? null : check_package(component.id, (etc as wzpackage).root[""]);

			components[18] = null;

			update_actions(property, property);
		}
		else if ("030" == component.type)
		{
			index = 18;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""][component.id];

			components[16] = null;

			properties[0] = new List<string>() { "sit", };
			properties[1] = components[1].property;
		}
		else
		{
			components_species species = etc as components_species;

			sign = component.id + "|" + species.type;

			if (0 == species.etc)
			{
				index = 13;

				if (!isequals(index, sign))
					return false;

				if (null != components[15])
					if (2 == components[15].etc)
						components[15] = null;
			}
			else
			{
				index = 15;

				if (!isequals(index, sign))
					return false;

				if (2 == species.etc)
					components[13] = null;
			}

			property = component.property[""];

			if ("0170" == component.type)
				if (!property.query(out property, species.type.Substring(2)))
					return false;

			if (null == components[16] && null == components[18])
				update_actions(components[1].property, property);
		}

		components[index] = new scenes_component(component, property, sign, null == etc ? 0 : (typeof(components_species) == etc.GetType()) ? (etc as components_species).etc : 0);

		return true;
	}

    //TODO: ANIMATION
	internal override momiji_animation generate_animation(string action, string emotion, bool cover, bool shadow, bool elf, string weapon, List<string> z)
	{
		List<string> frames;
		List<string> chairs = null;
		string taming = null;

		if (null != components[16])
		{
			frames = inspect_frames(action, property);

			if ("ladder" == action || "rope" == action)
			{
				wzproperty host;

				if (property.query(out host, action))
				{
					foreach (wzproperty frame in host)
						foreach (wzproperty imagery in frame)
							if (5 == imagery.type)
							{
								wzproperty canvas = inspect_canvas(imagery);

								if (null != canvas)
								{
									action = canvas.host.host.identity;
									taming = "sit";
								}

								break;
							}

					if (null == taming)
						if ("ladder" == action)
							taming = "rope";
						else
							taming = action;
				}
				else
				{
					if ("ladder" == action)
						taming = "rope";
					else
						taming = action;
				}
			}
			else
			{
				taming = "sit";
			}
		}
		else if (null != components[18])
		{
			int maximum = 0;

			frames = inspect_frames("effect", components[18].property);

			if (null != frames)
			{
				chairs = frames;
				maximum = frames.Count;

				frames = inspect_frames("effect2", components[18].property);

				if (null != frames)
					if (frames.Count > maximum)
					{
						chairs = frames;
						maximum = frames.Count;
					}

				frames = inspect_frames(action, property);
				taming = action;

				maximum = maximum - frames.Count;

				while (0 < maximum--)
					frames.Add(frames[0]);
			}
		}
		else
		{
			frames = inspect_frames(action, property);

			if (action.StartsWith("stand") || action.StartsWith("alert"))
				frames.Add(frames[1]);
		}

		if (null != frames)
		{
			Rectangle rectangle;
			momiji_animation animation;
			int[] delays2 = null;
			int[] delays1 = new int[frames.Count];
			int[] faces = new int[frames.Count];
			List<scenes_fragment>[] fragmentss = new List<scenes_fragment>[frames.Count];
			Dictionary<string, scenes_character_fragment>[][] rawsss = new Dictionary<string, scenes_character_fragment>[max][];

			for (int component = 0; component < max; ++component)
				if (2 == component || 5 == component)
				{
					if (null != components[component])
					{
						wzproperty grand;
						List<string> emotions = inspect_frames(emotion, components[component].property);

						if (components[component].property.query(out grand, emotion))
						{
							Dictionary<string, scenes_character_fragment>[] rawss = new Dictionary<string, scenes_character_fragment>[emotions.Count];

							if (2 == component)
								delays2 = new int[emotions.Count];
							else
								if (null == delays2)
								{
									delays2 = new int[emotions.Count];

									for (int index = 0; index < emotions.Count; ++index)
										delays2[index] = 100;
								}

							for (int index = 0; index < emotions.Count; ++index)
							{
								wzproperty host = inspect_property(grand[emotions[index]]);

								rawss[index] = new Dictionary<string, scenes_character_fragment>();

								if (null != host)
									foreach (wzproperty property in host)
									{
										wzproperty fragment;
										wzproperty canvas = inspect_canvas(property);

									/*	if (null == canvas)
										{
											canvas = inspect_property(property);

											if (null != canvas)
												canvas.query("0", out canvas);
										}*/

										if (null != canvas)
										{
											fragment = inspect_canvas(canvas);

											if (null != fragment)
												rawss[index].Add(property.identity, new scenes_character_fragment(inspect_canvas(fragment), inspect_canvas1(fragment), z));
										}
									}

								if (2 == component)
									delays2[index] = host.query(100, "delay");
							}

							rawsss[component] = rawss;
						}

					}
				}
				else if (18 == component)
				{
					if (null != components[component])
					{
						Dictionary<string, scenes_character_fragment>[] rawss = new Dictionary<string, scenes_character_fragment>[frames.Count];

						foreach (wzproperty property in components[component].property)
							if ("effect" == property.identity || "effect2" == property.identity)
							{
								int zorder;
								string zc = property.query<string>(null as string, "z");

								if (null == zc)
								{
									zorder = -2;
								}
								else
								{
									zorder = int.Parse(zc);

									if (-1 == zorder)
										zorder = -2;
									else if (0 == zorder)
										zorder = z.Count;
									else
										zorder = z.Count + 1;
								}

								for (int index = 0; index < frames.Count; ++index)
								{
									wzproperty fragment;

									if (null == rawss[index])
										rawss[index] = new Dictionary<string, scenes_character_fragment>();

									if (!property.query(out fragment, chairs[index]))
										if (!property.query(out fragment, chairs[0]))
											continue;

									rawss[index].Add(property.identity, new scenes_character_fragment(inspect_canvas(fragment), inspect_canvas1(fragment), zorder));
								}
							}

						rawsss[component] = rawss;
					}
				}
				else
				{
					if (null != components[component])
					{
						wzproperty grand;

						if (components[component].property.query(out grand, null == taming ? action : 16 == component || 17 == component || 18 == component ? action : taming))
						{
							Dictionary<string, scenes_character_fragment>[] rawss = new Dictionary<string, scenes_character_fragment>[frames.Count];

							if (5 == grand.type)
								grand = inspect_property(grand);

							for (int index = 0; index < frames.Count && null != grand; ++index)
							{
								wzproperty host;

								rawss[index] = new Dictionary<string, scenes_character_fragment>();

								if (grand.query(out host, frames[null == taming ? index : 16 == component || 17 == component || 18 == component ? index : 0]))
								{
									foreach (wzproperty property in host)
									{
										wzproperty fragment = inspect_canvas(property);

										if (null == fragment)
											fragment = inspect_property(property);

										if (null != fragment)
										{
											if ("ear" == property.identity)
											{
                                                 if (!elf)
                                                 continue;

                                            }
											else if ("hairOverHead" == property.identity)
											{
												if (cover)
													continue;
											}
											else if ("hair" == property.identity)
											{
												if (!cover)
													continue;
											}
											else if ("hairShade" == property.identity)
											{
												wzproperty shade;

												if (!shadow)
													continue;

												if (fragment.query(out shade, "0"))
													fragment = shade;
											}
											else if ("backHairOverCape" == property.identity)
											{
												if (cover)
													continue;
											}
											else if ("backHair" == property.identity)
											{
												if (cover)
													continue;
											}
											else if ("backHairBelowCap" == property.identity)
											{
												if (!cover)
													continue;
											}
											else if (property.identity.StartsWith("weapon"))
											{
												int value;

												if ("" == weapon)
												{
													if (int.TryParse(property.identity.Replace("weapon", ""), out value))
														continue;
												}
												else
												{
													if ("weapon" + weapon != property.identity)
														if (null != host["weapon" + weapon])
															continue;
														else if (int.TryParse(property.identity.Replace("weapon", ""), out value))
															continue;
												}
											}

											fragment = inspect_canvas(fragment);

											if (null != fragment)
												rawss[index].Add(property.identity, new scenes_character_fragment(fragment, inspect_canvas1(fragment), z));
										}
									}

									if (1 == component)
									{
										delays1[index] = host.query(100, "delay");
										faces[index] = host.query(0, "face");
									}
								}
							}

							rawsss[component] = rawss;
						}
					}
				}

			for (int index = 0; index < frames.Count; ++index)
				fragmentss[index] = new List<scenes_fragment>();

			if (null == rawsss[16])
			{
				adjust_fragments("body", frames.Count, 1, 1, rawsss, fragmentss);
			}
			else
			{
				adjust_fragments("0", frames.Count, 16, 16, rawsss, fragmentss);

				if (null != rawsss[17])
					adjust_fragments("0", frames.Count, 16, 17, rawsss, fragmentss);

				adjust_fragments("0", frames.Count, 16, 1, rawsss, fragmentss);
			}

			adjust_fragments("body", frames.Count, 1, 0, rawsss, fragmentss); // head

			if (null != rawsss[3])
				adjust_fragments("head", frames.Count, 0, 3, rawsss, fragmentss); // hair
			if (null != rawsss[4])
				adjust_fragments("head", frames.Count, 0, 4, rawsss, fragmentss); // cap
			if (null != rawsss[6])
				adjust_fragments("head", frames.Count, 0, 6, rawsss, fragmentss); // eye accessory
			if (null != rawsss[7])
				adjust_fragments("head", frames.Count, 0, 7, rawsss, fragmentss); // ear accessory

			if (null != rawsss[8])
				adjust_fragments("body", frames.Count, 1, 8, rawsss, fragmentss); // coat
			if (null != rawsss[9])
				adjust_fragments("body", frames.Count, 1, 9, rawsss, fragmentss); // longcoar
			if (null != rawsss[10])
				adjust_fragments("body", frames.Count, 1, 10, rawsss, fragmentss); // pants
			if (null != rawsss[11])
				adjust_fragments("body", frames.Count, 1, 11, rawsss, fragmentss); // shoes

			if (null != rawsss[12])
				adjust_fragments("arm", frames.Count, 1, 12, rawsss, fragmentss); // glove
			if (null != rawsss[13])
				adjust_fragments("arm", frames.Count, 1, 13, rawsss, fragmentss); // shield

			if (null != rawsss[14])
				adjust_fragments("body", frames.Count, 1, 14, rawsss, fragmentss); // cape

			if (null != rawsss[15])
				adjust_fragments("arm", frames.Count, 1, 15, rawsss, fragmentss); // weapon

			if (null != rawsss[18])
				for (int index = 0; index < frames.Count; ++index)
					foreach (scenes_character_fragment raw in rawsss[18][index].Values)
						fragmentss[index].Add(new scenes_fragment(raw.canvas, raw.origin, raw.z));

			if (null == rawsss[2] && null == rawsss[5])
			{
				rectangle = compute_rectangle(new List<scenes_fragment>[][] { fragmentss, });
				animation = new momiji_animation(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, delays1, null);

				animation.Add(amalgamate_fragments(fragmentss, rectangle));

				return animation;
			}
			else
			{
				int count = (null == rawsss[2]) ? rawsss[5].Length : rawsss[2].Length;
				List<scenes_fragment>[][] fragmentsss = new List<scenes_fragment>[count][];

				for (int index = 0; index < count; ++index)
				{
					Dictionary<string, scenes_character_fragment>[][] facesss = new Dictionary<string, scenes_character_fragment>[2][];
					Dictionary<string, scenes_character_fragment> face = (null == rawsss[2]) ? null : rawsss[2][rawsss[2].Length > index ? index : 0];
					Dictionary<string, scenes_character_fragment> accessory = (null == rawsss[5]) ? null : rawsss[5][rawsss[5].Length > index ? index : 0];

					facesss[0] = new Dictionary<string, scenes_character_fragment>[frames.Count];
					facesss[1] = new Dictionary<string, scenes_character_fragment>[frames.Count];
					fragmentsss[index] = new List<scenes_fragment>[frames.Count];

					for (int frame = 0; frame < frames.Count; ++frame)
					{
						facesss[0][frame] = new Dictionary<string, scenes_character_fragment>();
						facesss[1][frame] = new Dictionary<string, scenes_character_fragment>();
						fragmentsss[index][frame] = new List<scenes_fragment>(fragmentss[frame]);

						if (1 == faces[frame])
						{
							if (null != face)
								foreach (string key in face.Keys)
									facesss[0][frame].Add(key, face[key]);

							if (null != accessory)
								foreach (string key in accessory.Keys)
									facesss[1][frame].Add(key, accessory[key]);
						}
					}

					adjust_face(frames.Count, rawsss, facesss[0], fragmentsss[index]);
					adjust_face(frames.Count, rawsss, facesss[1], fragmentsss[index]);
				}

				rectangle = compute_rectangle(fragmentsss);
				animation = new momiji_animation(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, delays1, delays2);

				for (int index = 0; index < count; ++index)
					animation.Add(amalgamate_fragments(fragmentsss[index], rectangle));

				return animation;
			}
		}

		return null;
	}

	private scenes_component check_package(string id, wzproperty property)
	{
		wzproperty current;

		if (property.query(out current, id))
			return new scenes_component(current);

		if (property.query(out current, int.Parse(id).ToString()))
			return new scenes_component(current);

		return null;
	}

	private void adjust_fragments(string element, int count, int reference, int relative, Dictionary<string, scenes_character_fragment>[][] rawsss, List<scenes_fragment>[] fragmentss)
	{
		for (int index = 0; index < count; ++index)
		{
			string temp_element = element;

			if (!rawsss[reference][index].ContainsKey(temp_element))
				if (0 < rawsss[reference][index].Count)
					temp_element = new List<string>(rawsss[reference][index].Keys)[0];

			if ("0" == temp_element && rawsss[reference][index].ContainsKey("0") && rawsss[reference][index].ContainsKey("1"))
				if (null == rawsss[reference][index][temp_element].navel && null != rawsss[reference][index]["1"].navel)
					rawsss[reference][index][temp_element].navel = rawsss[reference][index]["1"].navel;
				else if (null != rawsss[reference][index][temp_element].navel && null == rawsss[reference][index]["1"].navel)
					rawsss[reference][index]["1"].navel = rawsss[reference][index][temp_element].navel;

			foreach (scenes_character_fragment raw in rawsss[relative][index].Values)
			{
				scenes_fragment fragment;

				if (rawsss[reference][index].ContainsKey(temp_element))
				{
					fragment = adjust_fragment(rawsss[reference][index][temp_element], raw);

					if (null != fragment)
					{
						fragmentss[index].Add(fragment);

						continue;
					}
				}

				if ("body" != temp_element && "0" != temp_element)
					adjust_fragments("body", count, 1, relative, rawsss, fragmentss);
			}
		}
	}

	private void adjust_face(int count, Dictionary<string, scenes_character_fragment>[][] rawsss, Dictionary<string, scenes_character_fragment>[] facess, List<scenes_fragment>[] fragmentss)
	{
		for (int index = 0; index < count; ++index)
			foreach (scenes_character_fragment raw in facess[index].Values)
			{
				scenes_fragment fragment;

				if (rawsss[0][index].ContainsKey("head"))
				{
					fragment = adjust_fragment(rawsss[0][index]["head"], raw);

					if (null != fragment)
					{
						fragmentss[index].Add(fragment);

						continue;
					}
				}
			}
	}

	private scenes_fragment adjust_fragment(scenes_character_fragment reference, scenes_character_fragment relative)
	{
		if (null != reference.neck && null != relative.neck)
			relative.vector = reference.neck - relative.neck;
		else if (null != reference.brow && null != relative.brow)
			relative.vector = reference.brow - relative.brow;
		else if (null != reference.navel && null != relative.navel)
			relative.vector = reference.navel - relative.navel;
		else if (null != reference.hand && null != relative.hand)
			relative.vector = reference.hand - relative.hand;
		else
			if (relative.handMove)
				relative.vector = new wzvector(0, 0);
			else
				return null;

		relative.vector = relative.vector + reference.vector;

		return new scenes_fragment(relative.canvas, relative.origin - relative.vector, relative.z);
	}

	private class scenes_character_fragment
	{
		private object[] properties;

		internal wzcanvas canvas
		{
			get
			{
				return properties[0] as wzcanvas;
			}
		}

		internal wzvector origin
		{
			get
			{
				return inspect_vector(properties[1]);
			}
		}

		internal wzvector neck
		{
			get
			{
				return inspect_vector(properties[2]);
			}
		}

		internal wzvector brow
		{
			get
			{
				return inspect_vector(properties[3]);
			}
		}

		internal wzvector navel
		{
			get
			{
				return inspect_vector(properties[4]);
			}
			set
			{
				properties[4] = value;
			}
		}

		internal wzvector hand
		{
			get
			{
				return inspect_vector(properties[5]);
			}
		}

		internal bool handMove
		{
			get
			{
				return (bool)properties[6];
			}
		}

		internal wzvector vector
		{
			get
			{
				return inspect_vector(properties[7]);
			}
			set
			{
				properties[7] = value;
			}
		}

		internal int z
		{
			get
			{
				return (int)properties[8];
			}
		}

		internal scenes_character_fragment(wzproperty fragment, wzcanvas canvas1, int z)
		{
			wzcanvas canvas = canvas1 ?? fragment.data as wzcanvas;
			wzvector origin = fragment.query(new wzvector(-canvas.width, -canvas.height), "origin");

			properties = new object[] { canvas, new wzvector(origin.x, origin.y), null, null, null, null, false, null, z, };
		}

		internal scenes_character_fragment(wzproperty fragment, wzcanvas canvas1, List<string> z)
		{
			wzcanvas canvas = canvas1 ?? fragment.data as wzcanvas;
			wzvector origin = fragment.query(new wzvector(-canvas.width, -canvas.height), "origin");
			wzvector neck = fragment.query<wzvector>(null, "map", "neck");
			wzvector brow = fragment.query<wzvector>(null, "map", "brow");
			wzvector navel = fragment.query<wzvector>(null, "map", "navel");
			wzvector hand = fragment.query<wzvector>(null, "map", "hand");
			bool handMove = (null != fragment["map", "handMove"]);
			int zorder = -1;
			string zc = fragment.query<string>(null as string, "z");

			if (null != zc)
			{
				int digit;

				zorder = z.IndexOf(zc);

				if (0 > zorder && int.TryParse(zc, out digit))
					zorder = z.Count;
			}

			properties = new object[] { canvas, new wzvector(origin.x, origin.y), neck, brow, navel, hand, handMove, null, zorder, };
		}

		private wzvector inspect_vector(object value)
		{
			if (null != value)
			{
				wzvector vector = value as wzvector;

				return new wzvector(vector.x > 65535 ? 0 : vector.x, vector.y > 65535 ? 0 : vector.y);
			}

			return null;
		}
	}
}

internal class scenes_pet : scenes_scene_base
{
	internal static scenes_pet analyze_code(string[] entries, components_object component, components_genuses genuses)
	{
		scenes_pet scene;
		string essential;

		if (null == component.package)
			return null;

		scene = new scenes_pet(component);
		essential = component.id;

		for (int id, index = 1; index < entries.Length; ++index)
			if (essential != entries[index] && int.TryParse(entries[index], out id))
			{
				components_genus genus;

				if (genuses.TryGetValue("pet", out genus))
					foreach (components_species species in genus.Values)
						if ("500" != species.type)
							if (species.objects.TryGetValue(entries[index], out component))
							{
								scene.update_component(component, species);

								break;
							}
			}

		return scene;
	}

	protected override int max
	{
		get
		{
			return 2;
		}
	}

	internal override string code
	{
		get
		{
			return generate_code("p");
		}
	}

	internal scenes_pet(components_object component)
		: base(component)
	{
		update_actions(components[0].property, components[0].property);
	}

	internal override bool update_component(components_object component, object etc)
	{
		int index;
		string sign;
		wzproperty property;

		if ("500" == component.type)
		{
			index = 0;
			sign = component.id;

			if (!isequals(index, sign))
				return false;

			property = component.property[""];

			if (null != components[1])
				if (!components[1].component.etc.Contains(component.id))
					components[1] = null;

			update_actions(property, property);
		}
		else if ("0180" == component.type)
		{
			index = 1;
			sign = component.id;
			/*
			if (!isequals(index, sign))
				return false;
			*/
			if (!component.property[""].query(out property, components[0].id))
				return false;
		}
		else
		{
			return false;
		}

		components[index] = new scenes_component(component, property, sign, (typeof(components_species) == etc.GetType()) ? (etc as components_species).etc : 0);

		return true;
	}

	internal override momiji_animation generate_animation(string action, string emotion, bool cover, bool shadow, bool elf, string weapon, List<string> z)
	{
		List<string> frames = inspect_frames(action, property);

		if (null != frames)
		{
			Rectangle rectangle;
			momiji_animation animation;
			int[] delays = new int[frames.Count];
			List<scenes_fragment>[] fragmentss = new List<scenes_fragment>[frames.Count];

			for (int index = 0; index < frames.Count; ++index)
				fragmentss[index] = new List<scenes_fragment>();

			foreach (scenes_component component in components)
				if (null != component)
				{
					wzproperty host;

					if (inspect_property(component.property).query(out host, action))
					{
						wzproperty[] fragments = new wzproperty[frames.Count];

						for (int index = 0; index < frames.Count; ++index)
						{
							wzproperty fragment;

							if (host.query(out fragment, frames[index]))
							{
								fragmentss[index].Add(convert_to_fragment(fragment, 0));

								if ("500" == component.type)
									delays[index] = fragment.query(100, "delay");
							}
						}
					}
				}

			rectangle = compute_rectangle(new List<scenes_fragment>[][] { fragmentss, });
			animation = new momiji_animation(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, delays, null);

			animation.Add(amalgamate_fragments(fragmentss, rectangle));

			return animation;
		}

		return null;
	}

	private scenes_fragment convert_to_fragment(wzproperty raw, int z)
	{
		if (null != raw)
		{
			wzproperty fragment = inspect_canvas(raw);
		//	wzcanvas canvas = fragment.data as wzcanvas;
			wzcanvas canvas = inspect_canvas1(fragment) ?? fragment.data as wzcanvas;
			wzvector origin = fragment.query(new wzvector(-canvas.width, -canvas.height), "origin");

			return new scenes_fragment(canvas, new wzvector(origin.x, origin.y), fragment.query(z, "z"));
		}

		return null;
	}
}

internal abstract class scenes_scene_base
{
	protected abstract int max
	{
		get;
	}

	internal abstract string code
	{
		get;
	}

	internal abstract bool update_component(components_object component, object etc);

	internal abstract momiji_animation generate_animation(string action, string emotion, bool cover, bool shadow, bool elf, string weapon, List<string> z);

	protected object[] properties;

	protected scenes_component[] components;

	protected wzpackage packages;

	internal List<string> actions
	{
		get
		{
			return properties[0] as List<string>;
		}
	}

	protected wzproperty property
	{
		get
		{
			return properties[1] as wzproperty;
		}
	}

	internal scenes_component this[int index]
	{
		get
		{
			return components.Length > index ? components[index] : null;
		}
	}

	internal bool this[string identity]
	{
		get
		{
			foreach (scenes_component component in components)
				if (null != component)
					if (null != component.component)
						if (identity == component.id)
							return true;

			return false;
		}
	}

	protected scenes_scene_base(components_object component)
	{
		properties = new object[3];
		components = new scenes_component[max];

		components[0] = new scenes_component(component, component.property[""], component.id, 0);

		packages = component.package;

		while (null != packages.host)
			packages = packages.host;
	}

	protected void update_actions(wzproperty primary, wzproperty secondary)
	{
		List<string> actions = new List<string>();
		List<string> raws = new List<string>(primary.identities);

		foreach (string action in secondary.identities)
			if (raws.Contains(action))
			{
				wzproperty frame;

				if (primary[action].query(out frame, "0"))
				{
					if ("500" == components[0].type)
					{
						if (null == inspect_canvas(frame))
							continue;
					}
					else
					{
						bool valid = false;

						if (null == inspect_property(frame))
							continue;

						foreach (wzproperty fragment in frame)
							if (null != inspect_canvas(fragment))
							{
								valid = true;

								break;
							}

						if (!valid)
							continue;
					}

					actions.Add(action);
				}
			}

		properties[0] = actions;
		properties[1] = primary;
	}

	protected void update_emotions(wzproperty property)
	{
		List<string> emotions = new List<string>();

		foreach (string emotion in property.identities)
			if ("info" != emotion && "default" != emotion)
				emotions.Add(emotion);

		properties[2] = emotions;
	}

	internal void update_listview(ListView listview)
	{
		int index = 0;
		Point offset = listview.AutoScrollOffset;

		listview.BeginUpdate();

		listview.Items.Clear();

		listview.LargeImageList.Images.Clear();

		foreach (scenes_component component in components)
			if (null != component)
				if (null != component.component)
				{
					ListViewItem item = new ListViewItem(new string[] { component.id, 0 < component.name.Length ? component.name : "<null>", }, index++);

					item.Tag = component;
					item.ToolTipText = component.name;

					listview.Items.Add(item);

					listview.LargeImageList.Images.Add(component.icon);
				}

		listview.AutoScrollOffset = offset;

		listview.EndUpdate();
	}

	internal bool delete_component(scenes_component component)
	{
		for (int index = 1; index < max; ++index)
			if (component == components[index])
			{
				components[index] = null;

				if (2 == index)
				{
					if (null != components[5])
						update_emotions(components[5].property);
					else
						properties[2] = null;

					return true;
				}

				if (5 == index)
				{
					if (null == components[2])
						properties[2] = null;

					return true;
				}

				if (13 == index)
				{
					if (null != components[15])
					{
						return true;
					}
				}
				else if (15 == index)
				{
					if (null != components[16] || null != components[18])
					{
						return true;
					}
					else if (null != components[13])
					{
						update_actions(components[1].property, components[13].property);

						return true;
					}
				}
				else if (16 == index || 18 == index)
				{
					components[17] = null;

					if (null != components[15])
					{
						update_actions(components[1].property, components[15].property);

						return true;
					}
					else if (null != components[13])
					{
						update_actions(components[1].property, components[13].property);

						return true;
					}
				}
				else
				{
					return true;
				}

				update_actions(components[1].property, components[0].property);

				return true;
			}

		return false;
	}

	protected bool isequals(int index, string sign)
	{
		if (null != components[index])
			if (sign == components[index].sign)
				return false;

		return true;
	}

	protected string generate_code(string prefix)
	{
		if (null != components)
		{
			string surfix = "";

			foreach (scenes_component component in components)
				if (null != component)
					if (null != component.component)
						surfix = surfix + "," + component.sign;

			if (0 < surfix.Length)
				return prefix + surfix;
		}

		return "";
	}

	protected List<string> inspect_frames(string action, wzproperty property)
	{
		property = inspect_property(property[action]);

		if (null != property)
		{
			int digit;
			List<string> frames = new List<string>();

			foreach (string frame in property.identities)
				if (int.TryParse(frame, out digit))
					frames.Add(frame);

			if (0 < frames.Count)
				return frames;
		}

		return null;
	}

	protected List<Bitmap> amalgamate_fragments(List<scenes_fragment>[] fragmentss, Rectangle rectangle)
	{
		List<Bitmap> images = new List<Bitmap>();

		foreach (List<scenes_fragment> fragments in fragmentss)
		{
			Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
			Graphics graphics = Graphics.FromImage(bitmap);

			if ("500" != components[0].type)
				fragments.Sort(delegate(scenes_fragment a, scenes_fragment b)
				{
					return b.z == a.z ? 0 : b.z < a.z ? 1 : -1;
				});

			foreach (scenes_fragment fragment in fragments)
				if (null != fragment)
					graphics.DrawImage(fragment.image, -fragment.x - rectangle.Left, -fragment.y - rectangle.Top);

			graphics.Dispose();

			images.Add(bitmap);
		}

		return images;
	}

	protected Rectangle compute_rectangle(List<scenes_fragment>[][] fragmentsss)
	{
		Rectangle? rectangle = null;

		foreach (List<scenes_fragment>[] fragmentss in fragmentsss)
			foreach (List<scenes_fragment> fragments in fragmentss)
				foreach (scenes_fragment fragment in fragments)
					if (null != fragment)
					{
						wzcanvas canvas = fragment.canvas;
						wzvector vector = fragment.vector;
						Rectangle current = new Rectangle(-vector.x, -vector.y, canvas.width, canvas.height);

						rectangle = (null == rectangle) ? current : Rectangle.Union((Rectangle)rectangle, current);
					}

		return null == rectangle ? Rectangle.Empty : (Rectangle)rectangle;
	}

	protected wzproperty inspect_property(wzproperty property)
	{
		if (null != property)
		{
			if (3 == property.type)
				return property;
			if (5 == property.type)
				return inspect_property((property.data as wzuol).target);
		}

		return null;
	}

	protected wzproperty inspect_canvas(wzproperty property)
	{
		if (null != property)
		{
			if (4 == property.type)
				return property;
			if (5 == property.type)
				return inspect_canvas((property.data as wzuol).target);
		}

		return null;
	}

	protected wzcanvas inspect_canvas1(wzproperty property)
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

	protected class scenes_fragment
	{
		private object[] properties;

		internal wzcanvas canvas
		{
			get
			{
				return properties[0] as wzcanvas;
			}
		}

		internal Image image
		{
			get
			{
				return canvas.image;
			}
		}

		internal wzvector vector
		{
			get
			{
				return properties[1] as wzvector;
			}
		}

		internal int x
		{
			get
			{
				return vector.x;
			}
		}

		internal int y
		{
			get
			{
				return vector.y;
			}
		}

		internal int z
		{
			get
			{
				return (int)properties[2];
			}
		}

		internal scenes_fragment(wzcanvas canvas, wzvector vector, int z)
		{
			properties = new object[]
			{
				canvas,
				vector,
				z,
			};
		}
	}
}

internal class scenes_component
{
	private object[] properties;

	internal components_object component
	{
		get
		{
			return properties[0] as components_object;
		}
	}

	internal wzproperty property
	{
		get
		{
			return properties[1] as wzproperty;
		}
	}

	internal string sign
	{
		get
		{
			return properties[2] as string;
		}
	}

	internal int etc
	{
		get
		{
			return (int)properties[3];
		}
	}

	internal string type
	{
		get
		{
			return component.type;
		}
	}

	internal string id
	{
		get
		{
			return component.id;
		}
	}

	internal string name
	{
		get
		{
			return component.name;
		}
	}

	internal Image icon
	{
		get
		{
			string location;
			wzproperty icon = property;

			if ("0170" == component.type || "0180" == component.type)
				icon = icon.host;

			if ("0001" == component.type)
				location = "front\\head";
			else if ("0002" == component.type)
				location = "default\\face";
			else if ("0003" == component.type || "0004" == component.type)
				location = "default\\hairOverHead";
			else
				location = "info\\iconRaw";

			foreach (string entry in location.Split(new char[] { '\\', }, StringSplitOptions.RemoveEmptyEntries))
				if (!icon.query(out icon, entry))
					break;

			if (null == icon && ("0003" == component.type || "0004" == component.type))
			{
				icon = property;
				location = "default\\hair";

				foreach (string entry in location.Split(new char[] { '\\', }, StringSplitOptions.RemoveEmptyEntries))
					if (!icon.query(out icon, entry))
						break;
			}

			if (null != icon)
			{
				wzcanvas canvas = inspect_canvas1(icon);

				if (null != canvas)
					return resize_icon(canvas.image);
			}

			return resize_icon(momiji_owned_base.warning);
		}
	}

	internal scenes_component(wzproperty property)
	{
		properties = new object[]
		{
			null,
			property,
			0,
			0,
		};
	}

	internal scenes_component(components_object component, wzproperty property, string sign, int etc)
	{
		properties = new object[]
		{
			component,
			property,
			sign,
			etc,
		};
	}

	private wzproperty inspect_canvas(wzproperty property)
	{
		if (null != property)
		{
			if (4 == property.type)
				return property;
			if (5 == property.type)
				return inspect_canvas((property.data as wzuol).target);
		}

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
				wzpackage packages = component.package;

				while (null != packages.host)
					packages = packages.host;

				if (0 <= index)
					source = wzproperty.query<wzcanvas>(packages.query(link.Substring(0, index + 4).Split('/')), null, link.Substring(index + 5).Split('/'));
				else
					source = property.root.query<wzcanvas>(null, link.Split('/'));
			}

			return source;
		}

		return null;
	}

	private Image resize_icon(Image image)
	{
		Bitmap bitmap = new Bitmap(40, 40);
		Graphics graphics = Graphics.FromImage(bitmap);
		int left = (40 < image.Width) ? (image.Width - 40) / 2 : 0;
		int top = (40 < image.Height) ? (image.Height - 40) / 2 : 0;
		int width = (40 < image.Width) ? 40 : image.Width;
		int height = (40 < image.Height) ? 40 : image.Height;

		graphics.DrawImage(image, new Rectangle((40 - width) / 2, (40 - height) / 2, width, height), new Rectangle(left, top, width, height), GraphicsUnit.Pixel);

		graphics.Dispose();
		image.Dispose();

		return bitmap;
	}
}
