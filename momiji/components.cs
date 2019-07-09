using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

internal class components_host : momiji_owned_base
{
	private readonly static Dictionary<object, bool> expenses = new Dictionary<object, bool>()
	{
		{ 1, true },
		{ 0, true },
	};
	private readonly static Dictionary<object, bool> genders = new Dictionary<object, bool>()
	{
		{ 1, true },
		{ 0, true },
		{ 2, true },
	};
	private readonly static Dictionary<object, bool> occupations = new Dictionary<object, bool>()
	{
		{ 0, true },
		{ -1, true },
		{ 1, true },
		{ 2, true },
		{ 4, true },
		{ 8, true },
		{ 16, true },
	};

	private ToolStripComboBox item_keyword;
	private ToolStripButton item_search;
	private ToolStripSeparator item_separator_search_style;
	private ToolStripMenuItem item_style_tile;
	private ToolStripMenuItem item_style_icon;
	private ToolStripDropDownButton item_style;

	private ToolStripButton item_cash;
	private ToolStripButton item_standard;
	private ToolStripSeparator item_separator_standard_female;
	private ToolStripButton item_female;
	private ToolStripButton item_male;
	private ToolStripButton item_neutral;
	private ToolStripSeparator item_separator_neutral_unlimited;
	private ToolStripButton item_unlimited;
	private ToolStripButton item_beginner;
	private ToolStripButton item_warrior;
	private ToolStripButton item_magician;
	private ToolStripButton item_bowman;
	private ToolStripButton item_thief;
	private ToolStripButton item_pirate;
	private ToolStripSeparator item_separator_pirate_color;
	private ToolStripComboBox item_color;

	private SplitContainer hsplitcontainer;
	private momiji_listview category;
	private momiji_toolstrip arrangement;
	private momiji_tabcontrol tabcontrol;
	private momiji_listview objects;
	private momiji_toolstrip filter;

	private scenes_host scenes;

	private components_sources sources_inernal;
	private components_genuses genuses_inernal;

	private components_colors face;
	private components_colors hair;

	internal components_genuses genuses
	{
		get
		{
			return genuses_inernal;
		}
	}

	internal components_object character_essential
	{
		get
		{
			components_genus genus;

			if (genuses_inernal.TryGetValue("host", out genus))
			{
				components_species species;

				if (genus.TryGetValue("0001", out species))
					if (0 < species.objects.Count)
					{
						components_object component;

						if (species.objects.TryGetValue("00012003", out component))
							return component;
						else
							return species.objects.Values[0];
					}
			}

			return null;
		}
	}

	internal components_object pet_essential
	{
		get
		{
			components_genus genus;

			if (genuses_inernal.TryGetValue("pet", out genus))
			{
				components_species species;

				if (genus.TryGetValue("500", out species))
					if (0 < species.objects.Count)
					{
						components_object component;

						if (species.objects.TryGetValue("5000004", out component))
							return component;
						else
							return species.objects.Values[0];
					}
			}

			return null;
		}
	}

	internal components_host(momiji_host host)
	{
		item_keyword = new ToolStripComboBox();
		item_search = new ToolStripButton(null, query_image("toolstrip_search"), toolstrip_search_click);
		item_separator_search_style = new ToolStripSeparator();
		item_style_tile = new ToolStripMenuItem(null, query_image("toolstrip_style_tile"), toolstrip_style_tile_click);
		item_style_icon = new ToolStripMenuItem(null, query_image("toolstrip_style_icon"), toolstrip_style_icon_click);
		item_style = new ToolStripDropDownButton(null, query_image("toolstrip_style_tile"), item_style_tile, item_style_icon);

		item_cash = new ToolStripButton(null, query_image("toolstrip_cash"), toolstrip_expense_click);
		item_standard = new ToolStripButton(null, query_image("toolstrip_standard"), toolstrip_expense_click);
		item_separator_standard_female = new ToolStripSeparator();
		item_female = new ToolStripButton(null, query_image("toolstrip_female"), toolstrip_gender_click);
		item_male = new ToolStripButton(null, query_image("toolstrip_male"), toolstrip_gender_click);
		item_neutral = new ToolStripButton(null, query_image("toolstrip_neutral"), toolstrip_gender_click);
		item_separator_neutral_unlimited = new ToolStripSeparator();
		item_unlimited = new ToolStripButton(null, query_image("toolstrip_unlimited"), toolstrip_occupation_click);
		item_beginner = new ToolStripButton(null, query_image("toolstrip_beginner"), toolstrip_occupation_click);
		item_warrior = new ToolStripButton(null, query_image("toolstrip_warrior"), toolstrip_occupation_click);
		item_magician = new ToolStripButton(null, query_image("toolstrip_magician"), toolstrip_occupation_click);
		item_bowman = new ToolStripButton(null, query_image("toolstrip_bowman"), toolstrip_occupation_click);
		item_thief = new ToolStripButton(null, query_image("toolstrip_thief"), toolstrip_occupation_click);
		item_pirate = new ToolStripButton(null, query_image("toolstrip_pirate"), toolstrip_occupation_click);
		item_separator_pirate_color = new ToolStripSeparator();
		item_color = new ToolStripComboBox();

		hsplitcontainer = new SplitContainer();
		category = new momiji_listview();
		arrangement = new momiji_toolstrip();
		tabcontrol = new momiji_tabcontrol();
		objects = new momiji_listview();
		filter = new momiji_toolstrip();

		hsplitcontainer.Dock = DockStyle.Fill;
		hsplitcontainer.Parent = this;

		category.FullRowSelect = true;
		category.MultiSelect = false;
		category.HideSelection = false;
		category.ShowGroups = true;
		category.Dock = DockStyle.Fill;
		category.HeaderStyle = ColumnHeaderStyle.None;
		category.View = View.Details;
		category.Parent = hsplitcontainer.Panel1;

		category.Columns.Add("");

		category.SelectedIndexChanged += category_selected_index_changed;

		tabcontrol.Dock = DockStyle.Fill;
		tabcontrol.Parent = hsplitcontainer.Panel2;

		tabcontrol.SelectedIndexChanged += tabcontrol_selected_index_changed;

		objects.FullRowSelect = true;
		objects.HideSelection = false;
		objects.MultiSelect = false;
		objects.ShowItemToolTips = true;
		objects.Visible = false;
		objects.Dock = DockStyle.Fill;
		objects.HeaderStyle = ColumnHeaderStyle.None;
		objects.View = View.Tile;
		objects.LargeImageList = new ImageList();
		objects.TileSize = new Size(125, 40);

		objects.Columns.Add("");
		objects.Columns.Add("");

		objects.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;
		objects.LargeImageList.ImageSize = new Size(40, 40);

		objects.SelectedIndexChanged += objects_selected_index_changed;

		arrangement.GripStyle = ToolStripGripStyle.Hidden;
		arrangement.Parent = hsplitcontainer.Panel2;

		arrangement.Items.AddRange(new ToolStripItem[]
		{
			item_keyword,
			item_search,
			item_separator_search_style,
			item_style,
		});

		item_search.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_style.DisplayStyle = ToolStripItemDisplayStyle.Image;

		item_keyword.FlatStyle = FlatStyle.Standard;

		item_keyword.SelectedIndexChanged += toolstrip_combobox_selected_index_changed;
		item_keyword.KeyDown += toolstrip_combobox_key_down;

		filter.Dock = DockStyle.Bottom;
		filter.GripStyle = ToolStripGripStyle.Hidden;
		filter.Parent = hsplitcontainer.Panel2;

		filter.Items.AddRange(new ToolStripItem[]
		{
			item_cash,
			item_standard,
			item_separator_standard_female,
			item_female,
			item_male,
			item_neutral,
			item_separator_neutral_unlimited,
			item_unlimited,
			item_beginner,
			item_warrior,
			item_magician,
			item_bowman,
			item_thief,
			item_pirate,
			item_separator_pirate_color,
			item_color,
		});

		item_color.AutoSize = false;

		item_cash.Tag = 1;
		item_standard.Tag = 0;
		item_female.Tag = 1;
		item_male.Tag = 0;
		item_neutral.Tag = 2;
		item_unlimited.Tag = 0;
		item_beginner.Tag = -1;
		item_warrior.Tag = 1;
		item_magician.Tag = 2;
		item_bowman.Tag = 4;
		item_thief.Tag = 8;
		item_pirate.Tag = 16;

		item_cash.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_standard.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_female.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_male.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_neutral.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_unlimited.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_beginner.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_warrior.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_magician.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_bowman.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_thief.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_pirate.DisplayStyle = ToolStripItemDisplayStyle.Image;

		item_color.Size = new Size(100, 15);

		item_cash.Checked = true;
		item_standard.Checked = true;
		item_female.Checked = true;
		item_male.Checked = true;
		item_neutral.Checked = true;
		item_unlimited.Checked = true;
		item_beginner.Checked = true;
		item_warrior.Checked = true;
		item_magician.Checked = true;
		item_bowman.Checked = true;
		item_thief.Checked = true;
		item_pirate.Checked = true;

		item_color.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;

		item_color.DropDownStyle = ComboBoxStyle.DropDownList;
		item_color.FlatStyle = FlatStyle.Standard;

		item_color.SelectedIndexChanged += toolstrip_color_selected_index_changed;

		item_color.ComboBox.DrawItem += toolstrip_color_draw_item;

		show_genders(false);
		show_occupations(false);
		show_colors(false, null);

		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.Manual;
		Size = new Size(400, 300);

		FormClosing += host.owned_form_closing;

		if (momiji_preferences.general_explorer_style)
		{
			enable_explorer_style(category, true);
			enable_explorer_style(objects, true);
		}

		switch_language();
	}

	protected override void Dispose(bool disposing)
	{
		if (null != sources_inernal)
			sources_inernal.dispose();

		base.Dispose(disposing);
	}

	internal override void switch_language()
	{
		item_search.Text = momiji_languages.components_toolstrip_search;
		item_style.Text = momiji_languages.components_toolstrip_style;

		item_style_tile.Text = momiji_languages.components_toolstrip_style_tile;
		item_style_icon.Text = momiji_languages.components_toolstrip_style_icon;

		Text = momiji_languages.components_caption;

		switch_font();
	}

	internal override void update_preferences()
	{
		if (momiji_preferences.tgeneral_explorer_style != momiji_preferences.general_explorer_style)
		{
			enable_explorer_style(category, momiji_preferences.general_explorer_style);
			enable_explorer_style(objects, momiji_preferences.general_explorer_style);

			category.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			objects.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		if (momiji_preferences.tadvance_display_items != momiji_preferences.advance_display_items)
			pagination();
	}

	internal void initial(scenes_host scenes)
	{
		this.scenes = scenes;
	}

	internal void show(momiji_host host)
	{
		Point client = host.PointToScreen(Point.Empty);

		hsplitcontainer.SplitterDistance = 100;

		Location = new Point(client.X, client.Y + host.ClientSize.Height - Height);
		position = new Point(Left - host.Left, Top - host.Top);

		load_category();

		scenes.add_scene("character");

		Show(host);
	}

    //TODO: 加载配置选项
	private void load_category()
	{
		string category = momiji_host.assembly + "\\" + "momiji" + ".categories";
		string component = momiji_host.assembly + "\\" + "momiji" + ".components";
        string maplestory = momiji_host.assembly + "\\" + "mapleStory" + ".components";

        if (File.Exists(category) && File.Exists(component))
		{
			XmlDocument categories = new XmlDocument();
			XmlDocument components = new XmlDocument();
            
			categories.Load(category);
			components.Load(component);
            //追加XML
            
            if (File.Exists(maplestory))
            {
                XmlDocument maplestorys = new XmlDocument();
                maplestorys.Load(maplestory);
                parse_node(maplestorys["momiji"],components["momiji"]);
            }

            load_category(categories["momiji"], components["momiji"]);
		}
	}

    private void parse_node(XmlNode maplestorys, XmlNode components)
    {
        var insertNode = maplestorys.FirstChild;
        //解析拷贝文件
        foreach(XmlNode n in insertNode)
        {
            if (n.Name == "object")
            {
                var fileName = string.Format(@"{0}\thumbnails\character\{1}.png", momiji_host.assembly, n.Attributes["id"].Value);
                var destName = string.Format(@"{0}\thumbnails\character\{1}.png", momiji_host.assembly, n.Attributes["id"].Value.Replace("00011", "00012"));
                if (!File.Exists(fileName))
                {
                    try
                    {
                        File.Copy(destName, fileName);
                    }
                    catch { }
                }
            }
        }
        insertNode.Attributes["location"].Value = momiji_host.assembly + "\\" + "wz" + "\\";
        components.InsertBefore(components.OwnerDocument.ImportNode(insertNode, true), components.FirstChild);
    }
    
	private void load_category(XmlNode categories, XmlNode components)
	{
		components_objectses objectses = new components_objectses();

		sources_inernal = new components_sources();
		genuses_inernal = new components_genuses(categories, components, objectses, sources_inernal);

		face = new components_colors(categories["face"]);
		hair = new components_colors(categories["hair"]);

		foreach (components_genus genus in genuses_inernal.Values)
		{
			ListViewGroup group = new ListViewGroup(genus.name);

			group.Tag = genus;

			foreach (components_species species in genus.Values)
			{
				ListViewItem item = new ListViewItem(species.name, group);

				item.Tag = species;

				category.Items.Add(item);
			}

			category.Groups.Add(group);
		}

		category.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

		if (0 < category.Items.Count)
			category.Items[0].Selected = true;
	}

	private void show_genders(bool show)
	{
		item_separator_standard_female.Visible = show;
		item_female.Visible = show;
		item_male.Visible = show;
		item_neutral.Visible = show;
	}

	private void show_occupations(bool show)
	{
		item_separator_neutral_unlimited.Visible = show;
		item_unlimited.Visible = show;
		item_beginner.Visible = show;
		item_warrior.Visible = show;
		item_magician.Visible = show;
		item_bowman.Visible = show;
		item_thief.Visible = show;
		item_pirate.Visible = show;
	}

	private void show_colors(bool show, components_species species)
	{
		item_separator_pirate_color.Visible = show;
		item_color.Visible = show;

		if (show)
		{
			components_colors colors;
			int index;

			if ("0002" == species.type)
			{
				colors = face;
				index = 2;
			}
			else
			{
				colors = hair;
				index = 3;
			}

			item_color.Items.AddRange(colors.ToArray());

			item_color.SelectedIndex = (null == scenes[index] ? colors.index : int.Parse(scenes[index].id.Substring(species.etc, 1)));
			item_color.Tag = colors;
		}
		else
		{
			item_color.Tag = null;
		}
	}

	internal void update_focus()
	{
		if (0 < category.SelectedItems.Count)
			if ("0180" == (category.SelectedItems[0].Tag as components_species).type)
				pagination();
	}

	private void pagination()
	{
		item_color.Items.Clear();

		objects.Visible = false;
		objects.Tag = null;
		objects.Parent = null;

		objects.SelectedItems.Clear();

		objects.Items.Clear();

		objects.LargeImageList.Images.Clear();

		tabcontrol.selected_index = -1;

		tabcontrol.TabPages.Clear();

		if (0 < category.SelectedItems.Count)
			generate_objects(category.SelectedItems[0]);
	}

	private void generate_objects(ListViewItem item)
	{
		components_genus genus = item.Group.Tag as components_genus;
		components_species species = item.Tag as components_species;

		if ("host" == genus.type)
		{
			if ("0001" == species.type)
			{
				generate_objects(true, true, true, true, true, true, true, true, true, true, true, true, null, species);
			}
			else if ("0002" == species.type || "0003" == species.type || "0004" == species.type)
			{
				show_genders(true);
				show_colors(true, species);

				generate_objects(true, true, genders[item_female.Tag], genders[item_male.Tag], genders[item_neutral.Tag], true, true, true, true, true, true, true, null, species);
			}
		}
		else if ("install" == genus.type)
		{
			generate_objects(true, true, true, true, true, true, true, true, true, true, true, true, null, species);
		}
		else if ("pet" == genus.type)
		{
			string pet = ("0180" == species.type && "pet" == scenes.type) ? scenes[0].id : null;

			show_genders(true);

			generate_objects(expenses[item_cash.Tag], expenses[item_standard.Tag], genders[item_female.Tag], genders[item_male.Tag], genders[item_neutral.Tag], true, true, true, true, true, true, true, pet, species);
		}
		else
		{
			show_genders(true);
			show_occupations(true);

			generate_objects(expenses[item_cash.Tag], expenses[item_standard.Tag], genders[item_female.Tag], genders[item_male.Tag], genders[item_neutral.Tag], occupations[item_unlimited.Tag], occupations[item_beginner.Tag], occupations[item_warrior.Tag], occupations[item_magician.Tag], occupations[item_bowman.Tag], occupations[item_thief.Tag], occupations[item_pirate.Tag], null, species);
		}
	}

	private void generate_objects(bool cash, bool standard, bool female, bool male, bool neutral, bool unlimited, bool beginner, bool warrior, bool magician, bool bowman, bool thief, bool pirate, string pet, components_species species)
	{
		components_list list = new components_list(species.type, species.location);

		foreach (components_object component in species.objects.Values)
			if ((1 == component.cash && cash) || (0 == component.cash && standard))
				if (((1 == component.gender && female) || (0 == component.gender && male) || (2 == component.gender && neutral)))
				{
					if ("0170" == component.type)
						if (!component.etc.Contains(species.type.Substring(2)))
							continue;

					if ("0180" == component.type)
						if (!component.etc.Contains(pet))
							continue;

					if (0 == component.job)
					{
						if (!unlimited)
							continue;
					}
					else if (-1 == component.job)
					{
						if (!beginner)
							continue;
					}
					else if ((1 == (component.job & 1) && !warrior) || (2 == (component.job & 2) && !magician) || (4 == (component.job & 4) && !bowman) || (8 == (component.job & 8) && !thief) || (16 == (component.job) && !pirate))
					{
						continue;
					}

					list.Add(component.id, component);
				}

		if (0 < list.Count)
		{
			int ipp = momiji_preferences.advance_display_items;

			int index, pages = list.Count / ipp;

			for (index = 0; index < pages; ++index)
				tabcontrol.TabPages.Add((ipp * index + 1) + " - " + (ipp * (index + 1)));

			pages = list.Count % ipp;

			if (0 < pages)
				tabcontrol.TabPages.Add((ipp * index + 1) + " - " + (ipp * index + pages));

			objects.Tag = list;

			turn_page();
		}
	}

	private void turn_page()
	{
		if (null != objects.Tag && 0 <= tabcontrol.SelectedIndex)
		{
			components_list list = objects.Tag as components_list;
			int priority = momiji_preferences.advance_display_items * tabcontrol.SelectedIndex;

			for (int index = 0; index < momiji_preferences.advance_display_items && priority + index < list.Count; ++index)
			{
				components_object component = list.Values[priority + index];
				ListViewItem item = new ListViewItem(new string[] { component.id, 0 < component.name.Length ? component.name : "<null>", }, index);

				item.Tag = component;
				item.ToolTipText = component.name;

				objects.Items.Add(item);

				objects.LargeImageList.Images.Add(generate_icon(list.location, component.id));
			}

			if (0 < objects.Items.Count)
			{
				objects.Parent = tabcontrol.SelectedTab;
				objects.Visible = true;

				enable_explorer_style(objects, momiji_preferences.general_explorer_style);
			}
		}
	}

	private Image generate_icon(string location, string id)
	{
		location = momiji_host.assembly + "\\" + "thumbnails" + "\\" + location + "\\" + id + ".png";

		if (File.Exists(location))
			return resize_icon(Image.FromFile(location));

		return resize_icon(warning);
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

	private void search_target(string text, bool auto)
	{
		if (0 < text.Length && null != objects.Tag)
		{
			components_list list = objects.Tag as components_list;
			int selected = (0 < objects.SelectedItems.Count) ? list.IndexOfValue(objects.SelectedItems[0].Tag as components_object) : 0;

			for (int index = selected + 1; index < list.Count; ++index)
				if (search_listview(text, index, list, auto))
					return;

			for (int index = 0; index < selected; ++index)
				if (search_listview(text, index, list, auto))
					return;
		}
	}

	private bool search_listview(string text, int index, components_list list, bool auto)
	{
		string id = list.Values[index].id.ToLower();
		string name = list.Values[index].name.ToLower();
		string lower = text.ToLower();

		if (0 <= id.IndexOf(lower) || 0 <= name.IndexOf(lower))
		{
			int page;

			if (!auto)
				if (!item_keyword.Items.Contains(text))
					item_keyword.Items.Add(text);

			page = index / momiji_preferences.advance_display_items;

			if (0 <= page && tabcontrol.TabPages.Count > page)
			{
				tabcontrol.SelectedIndex = page;

				foreach (ListViewItem item in objects.Items)
					if (item.Tag == list.Values[index])
					{
						item.Selected = true;

						objects.EnsureVisible(item.Index);

						break;
					}
			}

			return true;
		}

		return false;
	}

	private void category_selected_index_changed(object o, EventArgs e)
	{
		show_genders(false);
		show_occupations(false);
		show_colors(false, null);

		pagination();
	}

	private void tabcontrol_selected_index_changed(object o, EventArgs e)
	{
		objects.Visible = false;
		objects.Parent = null;

		objects.SelectedItems.Clear();

		objects.Items.Clear();

		objects.LargeImageList.Images.Clear();

		turn_page();
	}

	private void toolstrip_combobox_selected_index_changed(object o, EventArgs e)
	{
		search_target(item_keyword.Text, false);

		item_keyword.Focus();
	}

	private void toolstrip_combobox_key_down(object o, KeyEventArgs e)
	{
		if (Keys.Enter == e.KeyCode)
		{
			search_target(item_keyword.Text, false);

			item_keyword.Focus();
		}
	}

	private void toolstrip_search_click(object o, EventArgs e)
	{
		search_target(item_keyword.Text, false);
	}

	private void toolstrip_style_tile_click(object o, EventArgs e)
	{
		objects.BeginUpdate();

		objects.View = View.Tile;

		objects.EndUpdate();

		item_style.Image = query_image("toolstrip_style_tile");
	}

	private void toolstrip_style_icon_click(object o, EventArgs e)
	{
		objects.BeginUpdate();

		objects.View = View.LargeIcon;

		objects.EndUpdate();

		item_style.Image = query_image("toolstrip_style_icon");
	}

	private void objects_selected_index_changed(object o, EventArgs e)
	{
		if (objects.Visible && 0 < category.SelectedItems.Count && 0 < objects.SelectedItems.Count)
			scenes.update_scene(
				objects.SelectedItems[0].Tag as components_object,
				category.SelectedItems[0].Tag as components_species,
				item_color.SelectedIndex,
				sources_inernal);
	}

	private void toolstrip_expense_click(object o, EventArgs e)
	{
		ToolStripButton button = o as ToolStripButton;

		button.Checked = !button.Checked;

		expenses[(int)button.Tag] = button.Checked;

		pagination();
	}

	private void toolstrip_gender_click(object o, EventArgs e)
	{
		ToolStripButton button = o as ToolStripButton;

		button.Checked = !button.Checked;

		genders[(int)button.Tag] = button.Checked;

		pagination();
	}

	private void toolstrip_occupation_click(object o, EventArgs e)
	{
		ToolStripButton button = o as ToolStripButton;

		button.Checked = !button.Checked;

		occupations[(int)button.Tag] = button.Checked;

		pagination();
	}

	private void toolstrip_color_selected_index_changed(object o, EventArgs e)
	{
		if (null != item_color.Tag)
			(item_color.Tag as components_colors).index = item_color.SelectedIndex;

		if (0 <= item_color.SelectedIndex)
			if ("character" == scenes.type)
			{
				scenes_component current;

				switch ((category.SelectedItems[0].Tag as components_species).type)
				{
					case "0002": current = scenes[2]; break;

					case "0003":
					case "0004": current = scenes[3]; break;

					default: return;
				}

				if (null != current)
					scenes.update_scene(
						current.component,
						category.SelectedItems[0].Tag as components_species,
						item_color.SelectedIndex,
						sources_inernal);
			}
	}

	private void toolstrip_color_draw_item(object o, DrawItemEventArgs e)
	{
		if (0 <= e.Index)
		{
			components_color color = item_color.Items[e.Index] as components_color;
			Rectangle rectangle = e.Bounds;
			SolidBrush brush = new SolidBrush(color.color);

			e.DrawBackground();

			e.Graphics.FillRectangle(brush, rectangle.Left + 1, rectangle.Top + 1, rectangle.Height - 2, rectangle.Height - 2);

			e.Graphics.DrawString(color.name, filter.Font, Brushes.Black, new RectangleF(rectangle.Left + rectangle.Height, rectangle.Top + 1, rectangle.Width - rectangle.Height, rectangle.Height));

			if (DrawItemState.Focus == e.State)
				e.DrawFocusRectangle();

			brush.Dispose();
		}
	}

	private class components_list : SortedList<string, components_object>
	{
		private object[] properties;

		internal string type
		{
			get
			{
				return properties[0] as string;
			}
		}

		internal string location
		{
			get
			{
				return properties[1] as string;
			}
		}

		internal components_list(string type, string location)
		{
			properties = new object[]
			{
				type,
				location,
			};
		}
	}

	private class components_colors : List<components_color>
	{
		private object[] properties;

		internal int index
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

		internal components_colors(XmlNode colors)
		{
			properties = new object[]
			{
				0,
			};

			foreach (XmlNode color in colors)
				if ("color" == color.Name)
					Add(new components_color(color));
		}
	}

	private class components_color
	{
		private object[] properties;

		internal string name
		{
			get
			{
				return properties[0] as string;
			}
		}

		internal string index
		{
			get
			{
				return properties[1] as string;
			}
		}

		internal Color color
		{
			get
			{
				return Color.FromArgb(255, Color.FromArgb(int.Parse(properties[2] as string)));
			}
		}

		internal components_color(XmlNode color)
		{
			properties = new object[]
			{
				color.Attributes["name"].Value,
				color.Attributes["index"].Value,
				color.Attributes["value"].Value,
			};
		}
	}
}

internal class components_genuses : Dictionary<string, components_genus>
{
	internal components_genuses(XmlNode categories, XmlNode components, components_objectses objectses, components_sources sources)
	{
		foreach (XmlNode genus in categories)
			if ("genus" == genus.Name)
				Add(genus.Attributes["type"].Value, new components_genus(genus, components, objectses, sources));
	}
}

internal class components_genus : Dictionary<string, components_species>
{
	private object[] properties;

	internal string name
	{
		get
		{
			return properties[0] as string;
		}
	}

	internal string type
	{
		get
		{
			return properties[1] as string;
		}
	}

	internal components_genus(XmlNode genus, XmlNode componentses, components_objectses objectses, components_sources sources)
	{
		properties = new object[]
		{
			genus.Attributes["name"].Value,
			genus.Attributes["type"].Value,
		};

		foreach (XmlNode species in genus)
			if ("species" == species.Name)
				Add(species.Attributes["type"].Value, new components_species(species, componentses, objectses, sources));
	}
}

internal class components_species
{
	private object[] properties;

	internal string name
	{
		get
		{
			return properties[0] as string;
		}
	}

	internal string type
	{
		get
		{
			return properties[1] as string;
		}
	}

	internal int etc
	{
		get
		{
			int value;

			int.TryParse(properties[2] as string, out value);

			return value;
		}
	}

	internal string location
	{
		get
		{
			return properties[3] as string;
		}
	}

	internal SortedList<string, components_object> objects
	{
		get
		{
			SortedList<string, components_object> list = new SortedList<string, components_object>();
			components_objects objects = properties[4] as components_objects;

			foreach (string id in objects.Keys)
				if (id.StartsWith(type) || id.StartsWith("0170"))
					list.Add(id, objects[id]);

			return list;
		}
	}

	internal components_object this[string identity]
	{
		get
		{
			int index = -1;

			if ("0002" == type)
				index = 5;
			else if ("0003" == type || "0004" == type)
				index = 7;
			else
				return objects.ContainsKey(identity) ? objects[identity] : null;

			identity = identity.Remove(index, 1);

			foreach (string source in objects.Keys)
				if (source.Remove(index, 1) == identity)
					return objects[source];

			return null;
		}
	}

	internal components_species(XmlNode species, XmlNode componentses, components_objectses objectses, components_sources sources)
	{
		properties = new object[]
		{
			species.Attributes["name"].Value,
			species.Attributes["type"].Value,
			species.Attributes["etc"].Value,
			species.Attributes["location"].Value,
			objectses.exists(species.Attributes["location"].Value, componentses, sources),
		};
	}
}

internal class components_objectses : Dictionary<string, components_objects>
{
	internal components_objects exists(string location, XmlNode componentses, components_sources sources)
	{
		components_objects objects;

		location = location.ToLower();

		if (!TryGetValue(location, out objects))
		{
			objects = new components_objects(location, componentses, sources);

			Add(location, objects);
		}

		return objects;
	}
}

internal class components_objects : Dictionary<string, components_object>
{
	private readonly static List<string> specieses = new List<string>()
	{
		"Accessory", "Cap", "Cape", "Coat", "Face", "Glove", "Hair",
		"Longcoat", "Pants", "PetEquip", "Shield", "Shoes", "TamingMob", "Weapon",
		"Install", "Pet"
	};

    //TODO: 解析包
	internal components_objects(string location, XmlNode componentses, components_sources sources)
	{
		string[] entries = location.Split(new char[] { '\\', }, StringSplitOptions.RemoveEmptyEntries);

		for (int index = 0; index < componentses.ChildNodes.Count; ++index)
		{
			XmlNode components = componentses.ChildNodes[index];

            if (entries[0] == components.Name)
            {
                wzpackage package = sources.exists(Path.GetFullPath(components.Attributes["location"].Value))[entries[0]];

                for (int entry = 1; entry < entries.Length; ++entry)
                {
                    components = components[entries[entry]];

                    if (null != package)
                        package = package[match_species(entries[entry])];
                }
                if (components != null)
                {
                    foreach (XmlNode component in components)
                    {
                        if ("object" == component.Name)
                        {
                            string id = component.Attributes["id"].Value;

                            if (!ContainsKey(id))
                                Add(id, new components_object(component, package));
                        }
                    }
                }
            }
		}
	}

	private string match_species(string origin)
	{
		foreach (string species in specieses)
			if (origin.ToLower() == species.ToLower())
				return species;

		return null;
	}
}

internal class components_object
{
	private object[] properties;

	internal string type
	{
		get
		{
			return properties[0] as string;
		}
	}

	internal string id
	{
		get
		{
			return properties[1] as string;
		}
	}

	internal string name
	{
		get
		{
			return properties[2] as string;
		}
	}

	internal int level
	{
		get
		{
			return int.Parse(properties[3] as string);
		}
	}

	internal int gender
	{
		get
		{
			return int.Parse(properties[4] as string);
		}
	}

	internal int cash
	{
		get
		{
			return int.Parse(properties[5] as string);
		}
	}

	internal int job
	{
		get
		{
			return int.Parse(properties[6] as string);
		}
	}

	internal List<string> etc
	{
		get
		{
			return properties[7] as List<string>;
		}
	}

	internal wzpackage package
	{
		get
		{
			return properties[8] as wzpackage;
		}
	}

	internal wzproperty property
	{
		get
		{
			return null == properties[8] ? null : (properties[8] as wzpackage).root;
		}
	}

	internal components_object(string id, components_object component, wzpackage package)
	{
		properties = new object[]
		{
			component.type,
			id,
			component.name,
			component.level.ToString(),
			component.gender.ToString(),
			component.cash.ToString(),
			component.job.ToString(),
			component.etc,
			package,
		};
	}

	internal components_object(XmlNode component, wzpackage package)
	{
		if (null != package)
			if ("030" == component.Attributes["type"].Value)
				package = package[component.Attributes["id"].Value.Substring(0, 4) + ".img"];
			else
				package = package[component.Attributes["id"].Value + ".img"];

		properties = new object[]
		{
			component.Attributes["type"].Value,
			component.Attributes["id"].Value,
			component.Attributes["name"].Value,
			component.Attributes["level"].Value,
			component.Attributes["gender"].Value,
			component.Attributes["cash"].Value,
			component.Attributes["job"].Value,
			new List<string>(component.Attributes["etc"].Value.Split(new char[] { ',', }, StringSplitOptions.RemoveEmptyEntries)),
			package,
		};
	}
}

internal class components_sources : Dictionary<string, components_source>
{
	internal components_source exists(string location)
	{
		components_source source;

		location = location.ToLower();

		if (!TryGetValue(location, out source))
		{
			source = new components_source(location);

			Add(location, source);
		}

		return source;
	}

	internal void dispose()
	{
		foreach (components_source source in Values)
			source.dispose();
	}
}

internal class components_source
{
	private List<string> specieses = new List<string>() { "character", "item", };

	private wzarchives archives;

	private wzpackage[] packages;

	internal wzpackage this[string species]
	{
		get
		{
			return null == packages ? null : packages[specieses.IndexOf(species)];
		}
	}

	internal components_source(string location)
	{
		/*	string character = location + (location.EndsWith("\\") ? "" : "\\") + "character.wz";
			string item = location + (location.EndsWith("\\") ? "" : "\\") + "item.wz";

			if (File.Exists(character) || File.Exists(item))
			{
				archives = new wzarchive[]
				{
					new wzarchive(character),
					new wzarchive(item),
				};

				packages = new wzpackage[]
				{
					archives[0].root,
					archives[1].root,
				};
			}*/

		string basewz = location + (location.EndsWith("\\") ? "" : "\\") + "base.wz";

		if (File.Exists(basewz))
		{
			archives = new wzarchives(basewz);

			packages = new wzpackage[]
			{
				archives.root["Character"],
				archives.root["Item"],
			};
		}
	}

	internal void dispose()
	{
		archives.dispose();
	}
}
