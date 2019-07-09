using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Globalization;

internal class actions_host : momiji_owned_base
{
	private Label action1;
	private Label action2;
	private ComboBox actions1;
	private ComboBox actions2;
	private ComboBox frames1;
	private ComboBox frames2;
	private CheckBox play1;
	private CheckBox play2;
	private CheckBox cover;
	private CheckBox shadow;
	private RadioButton weapon0;
	private RadioButton weapon1;
	private RadioButton weapon2;
	private RadioButton weapon3;
	private Timer timer;

	private List<actions_animation> animations;
	private actions_animation animation;

	private momiji_host momiji;

	internal scenes_container current
	{
		set
		{
			animation = null;

			if (null != value)
			{
				for (int index = 0; index < animations.Count; ++index)
					if (value == animations[index].container)
					{
						animation = animations[index];

						break;
					}

				if (null == animation)
				{
					animation = new actions_animation(value, play1.Checked, play2.Checked, cover.Checked, shadow.Checked, false, "");

					animations.Add(animation);

					momiji.add_animation(animation);
				}
			}

			update_actions();
		}
		get
		{
			return null == animation ? null : animation.container;
		}
	}

	internal actions_host(momiji_host host)
	{
		action1 = new Label();
		action2 = new Label();
		actions1 = new ComboBox();
		actions2 = new ComboBox();
		frames1 = new ComboBox();
		frames2 = new ComboBox();
		play1 = new CheckBox();
		play2 = new CheckBox();
		cover = new CheckBox();
		shadow = new CheckBox();

		weapon0 = new RadioButton();
		weapon1 = new RadioButton();
		weapon2 = new RadioButton();
		weapon3 = new RadioButton();
		timer = new Timer(container);

		action1.TextAlign = ContentAlignment.MiddleLeft;
		action1.Location = new Point(10, 10);
		action1.Size = new Size(50, 20);
		action1.Parent = this;

		action2.TextAlign = ContentAlignment.MiddleLeft;
		action2.Location = new Point(10, 40);
		action2.Size = new Size(50, 20);
		action2.Parent = this;

		actions1.DropDownStyle = ComboBoxStyle.DropDownList;
		actions1.Location = new Point(70, 10);
		actions1.Size = new Size(100, 20);
		actions1.Parent = this;

		actions1.SelectedIndexChanged += actions1_selected_index_changed;

		actions2.DropDownStyle = ComboBoxStyle.DropDownList;
		actions2.Location = new Point(70, 40);
		actions2.Size = new Size(100, 20);
		actions2.Parent = this;

		actions2.SelectedIndexChanged += actions2_selected_index_changed;

		frames1.DropDownStyle = ComboBoxStyle.DropDownList;
		frames1.Location = new Point(180, 10);
		frames1.Size = new Size(50, 20);
		frames1.Parent = this;

		frames1.SelectedIndexChanged += frames1_selected_index_changed;

		frames2.DropDownStyle = ComboBoxStyle.DropDownList;
		frames2.Location = new Point(180, 40);
		frames2.Size = new Size(50, 20);
		frames2.Parent = this;

		frames2.SelectedIndexChanged += frames2_selected_index_changed;

		play1.TextAlign = ContentAlignment.BottomLeft;
		play1.Location = new Point(240, 10);
		play1.Size = new Size(50, 20);
		play1.Parent = this;

		play1.Click += play1_click;

		play2.TextAlign = ContentAlignment.BottomLeft;
		play2.Location = new Point(240, 40);
		play2.Size = new Size(50, 20);
		play2.Parent = this;

		play2.Click += play2_click;

		cover.TextAlign = ContentAlignment.BottomLeft;
		cover.Location = new Point(10, 70);
		cover.Size = new Size(75, 20);
		cover.Parent = this;

		cover.Click += cover_click;

		shadow.TextAlign = ContentAlignment.BottomLeft;
		shadow.Location = new Point(95, 70);
		shadow.Size = new Size(85, 20);
		shadow.Parent = this;

		shadow.Click += shadow_click;

		enable_action(false);
		enable_emotion(false);
		enable_others(false);

		weapon0.AutoCheck = true;
		weapon0.Checked = true;
		weapon0.Appearance = Appearance.Button;
		weapon0.Location = new Point(270, 70);
		weapon0.Size = new Size(10, 10);
		weapon0.Tag = "";
		weapon0.Parent = this;

		weapon0.Click += weapon_click;

		weapon1.Appearance = Appearance.Button;
		weapon1.Location = new Point(280, 70);
		weapon1.Size = new Size(10, 10);
		weapon1.Tag = "1";
		weapon1.Parent = this;

		weapon1.Click += weapon_click;

		weapon2.Appearance = Appearance.Button;
		weapon2.Location = new Point(270, 80);
		weapon2.Size = new Size(10, 10);
		weapon2.Tag = "2";
		weapon2.Parent = this;

		weapon2.Click += weapon_click;

		weapon3.Appearance = Appearance.Button;
		weapon3.Location = new Point(280, 80);
		weapon3.Size = new Size(10, 10);
		weapon3.Tag = "3";
		weapon3.Parent = this;

		weapon3.Click += weapon_click;

		timer.Interval = 10;
		timer.Tag = 0;

		timer.Tick += timer_tick;

		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.Manual;
		ClientSize = new Size(300, 100);

		FormClosing += host.owned_form_closing;

		animations = new List<actions_animation>();
		momiji = host;

		switch_language();
	}

	internal override void switch_language()
	{
		action1.Text = momiji_languages.actions_action;
		action2.Text = momiji_languages.actions_emotion;
		play1.Text = momiji_languages.actions_play;
		play2.Text = momiji_languages.actions_play;
		cover.Text = momiji_languages.actions_cover;
		shadow.Text = momiji_languages.actions_shadow;

		Text = momiji_languages.actions_caption;

		switch_font();
	}

	internal void show(momiji_host host)
	{
		Point client = host.PointToScreen(Point.Empty);

		Location = new Point(client.X + host.ClientSize.Width - Width, client.Y + host.ClientSize.Height - Height);
		position = new Point(Left - host.Left, Top - host.Top);

		Show(host);
	}

	internal void flip_animation()
	{
		if (null != animation)
		{
			animation.flip_animation();

			momiji.update_canva();
		}
	}

	internal void save_animation(bool current)
	{
		string prefix = momiji_preferences.advance_output_location + "\\" + DateTime.Now.ToString("yyyy.MM.dd hh.mm.ss.fff");
		bool frames = momiji_preferences.advance_output_frames;
		int colors = momiji_preferences.advance_output_maxcolor;
		Color backcolor = momiji_preferences.advance_output_backcolor;

		for (int index = 0; index < animations.Count; ++index)
			if (current && animation != animations[index])
			{
				continue;
			}
			else
			{
				momiji_animation imagess = animations[index].animation;
				string location = string.Format("{0}[{1}]\\", prefix, index);

				Directory.CreateDirectory(location);

				if (Directory.Exists(location))
					if (frames)
						for (int emotion = 0; emotion < imagess.Count; ++emotion)
							for (int frame = 0; frame < imagess[emotion].Count; ++frame)
								imagess[emotion][frame].Save(location + emotion + ((null == imagess.delays2) ? "" : "," + imagess.delays2[emotion]) + "-" + frame + "," + imagess.delays1[frame] + ".png", ImageFormat.Png);
					else
						for (int emotion = 0; emotion < imagess.Count; ++emotion)
						{
							gif_encoder encoder = new gif_encoder(location + emotion + ((null == imagess.delays2) ? "" : "," + imagess.delays2[emotion]) + ".gif", imagess.width, imagess.height, colors, backcolor);

							for (int frame = 0; frame < imagess[emotion].Count; ++frame)
								encoder.append_frame(imagess[emotion][frame], imagess.delays1[frame]);

							encoder.destruct();
						}
			}
	}

	internal void delete_animation(int index)
	{
		actions_animation animation = animations[index];

		animations.RemoveAt(index);

		animation.dispose();
	}

	internal void move_animation(int index, int delta)
	{
		int position = index + delta;

		if (0 <= position && animations.Count > position)
		{
			actions_animation animation = animations[index];

			animations.RemoveAt(index);

			animations.Insert(position, animation);
		}
	}

	internal void update_actions()
	{
		if (Visible)
			SendMessage(Handle, 0x000b, 0, 0);

		timer.Enabled = false;

		enable_action(false);
		enable_emotion(false);
		enable_others(false);

		actions1.SelectedIndex = -1;
		actions2.SelectedIndex = -1;
		frames1.SelectedIndex = -1;
		frames2.SelectedIndex = -1;

		actions1.Items.Clear();
		actions2.Items.Clear();
		frames1.Items.Clear();
		frames2.Items.Clear();

		if (null != animation)
			if (0 < animation.actions.Count)
			{
				actions1.Items.AddRange(animation.actions.ToArray());

				if (animation.actions.Contains(animation.current1))
				{
					actions1.SelectedIndex = animation.actions.IndexOf(animation.current1);
				}
				else
				{
					int index;

					for (index = 0; index < animation.actions.Count; ++index)
						if (animation.actions[index].StartsWith("stand"))
						{
							actions1.SelectedIndex = index;

							break;
						}

					if (animation.actions.Count == index)
						actions1.SelectedIndex = 0;
				}

				enable_action(true);

				if (null != animation.emotions)
				{
					actions2.Items.AddRange(animation.emotions.ToArray());

					actions2.SelectedIndex = 0 > animation.emotions.IndexOf(animation.current2) ? 0 : animation.emotions.IndexOf(animation.current2);

					enable_emotion(true);
				}

				if ("character" == current.type)
					enable_others(true);

				timer.Enabled = true;
			}

		if (Visible)
			SendMessage(Handle, 0x000b, 1, 0);

		Refresh();
	}

	private void enable_action(bool enabled)
	{
		action1.Enabled = enabled;
		actions1.Enabled = enabled;
		frames1.Enabled = enabled;
		play1.Enabled = enabled;

		if (enabled)
			play1.Checked = animation.play1;
	}

	private void enable_emotion(bool enabled)
	{
		action2.Enabled = enabled;
		actions2.Enabled = enabled;
		frames2.Enabled = enabled;
		play2.Enabled = enabled;

		if (enabled)
			play2.Checked = animation.play2;
	}

	private void enable_others(bool enabled)
	{
		cover.Enabled = enabled;
		shadow.Enabled = enabled;

		weapon0.Enabled = enabled;
		weapon1.Enabled = enabled;
		weapon2.Enabled = enabled;
		weapon3.Enabled = enabled;

		if (enabled)
		{
			cover.Checked = animation.cover;
			shadow.Checked = animation.shadow;
			//TODO: EAR
            //elf.Checked = animation.elf;

			weapon0.Checked = false;
			weapon1.Checked = false;
			weapon2.Checked = false;
			weapon3.Checked = false;

			if ("" == animation.weapon)
				weapon0.Checked = true;
			else if ("1" == animation.weapon)
				weapon1.Checked = true;
			else if ("2" == animation.weapon)
				weapon2.Checked = true;
			else if ("3" == animation.weapon)
				weapon3.Checked = true;
		}
	}

	private void update_actions1()
	{
		frames1.SelectedIndex = -1;

		frames1.Items.Clear();

		if (null != animation.animation)
		{
			for (int index = 0; index < animation.count1; ++index)
				frames1.Items.Add((index + 1).ToString());

			if (0 < frames1.Items.Count)
				frames1.SelectedIndex = (animation.count1 > animation.frame1) ? animation.frame1 : 0;
		}
	}

	private void update_actions2()
	{
		frames2.SelectedIndex = -1;

		frames2.Items.Clear();

		if (null != animation.animation)
		{
			for (int index = 0; index < animation.count2; ++index)
				frames2.Items.Add((index + 1).ToString());

			if (0 < frames1.Items.Count)
				frames2.SelectedIndex = (animation.count2 > animation.frame2) ? animation.frame2 : 0;
		}
	}

	private void actions1_selected_index_changed(object o, EventArgs e)
	{
		if (null != animation && 0 <= actions1.SelectedIndex)
		{
			animation.current1 = animation.actions[actions1.SelectedIndex];

			animation.update_animation();

			update_actions1();
		}
	}

	private void actions2_selected_index_changed(object o, EventArgs e)
	{
		if (null != animation && 0 <= actions2.SelectedIndex)
		{
			animation.current2 = animation.emotions[actions2.SelectedIndex];

			animation.update_animation();

			update_actions2();
		}
	}

	private void frames1_selected_index_changed(object o, EventArgs e)
	{
		if (null != animation && 0 <= frames1.SelectedIndex)
		{
			animation.frame1 = frames1.SelectedIndex;

			momiji.update_canva();
		}
	}

	private void frames2_selected_index_changed(object o, EventArgs e)
	{
		if (null != animation && 0 <= frames2.SelectedIndex)
		{
			animation.frame2 = frames2.SelectedIndex;

			momiji.update_canva();
		}
	}

	private void play1_click(object o, EventArgs e)
	{
		if (null != animation)
			animation.play1 = play1.Checked;
	}

	private void play2_click(object o, EventArgs e)
	{
		if (null != animation)
			animation.play2 = play2.Checked;
	}

	private void cover_click(object o, EventArgs e)
	{
		if (null != animation)
		{
			animation.cover = cover.Checked;

			animation.update_animation();

			update_actions1();
		}
	}

	private void shadow_click(object o, EventArgs e)
	{
		if (null != animation)
		{
			animation.shadow = shadow.Checked;

			animation.update_animation();

			update_actions1();
		}
	}

	private void elf_click(object o, EventArgs e)
	{
		if (null != animation)
		{
			//animation.elf = elf.Checked;
            //TODO: ELF
			animation.update_animation();

			update_actions1();
		}
	}

	private void weapon_click(object o, EventArgs e)
	{
		if (null != animation)
		{
			animation.weapon = (o as Control).Tag as string;

			animation.update_animation();

			update_actions1();
		}
	}

	private void timer_tick(object o, EventArgs e)
	{
		foreach (actions_animation target in animations)
			if (null != target.animation)
			{
				if (target.play1 && null != target.actions)
				{
					if (target.ms1 >= target.delay1)
					{
						if (animation == target)
						{
							if (frames1.Items.Count <= frames1.SelectedIndex + 1)
								frames1.SelectedIndex = 0;
							else
								++frames1.SelectedIndex;
						}
						else
						{
							if (target.count1 <= target.frame1 + 1)
								target.frame1 = 0;
							else
								++target.frame1;

							momiji.update_canva();
						}

						target.ms1 = 0;
					}

					target.ms1 = target.ms1 + 10;
				}

				if (target.play2 && null != target.emotions)
				{
					if (target.ms2 >= target.delay2)
					{
						if (animation == target)
						{
							if (frames2.Items.Count <= frames2.SelectedIndex + 1)
								frames2.SelectedIndex = 0;
							else
								++frames2.SelectedIndex;
						}
						else
						{
							if (target.count2 <= target.frame2 + 1)
								target.frame2 = 0;
							else
								++target.frame2;

							momiji.update_canva();
						}

						target.ms2 = 0;
					}

					target.ms2 = target.ms2 + 10;
				}
			}
	}
}

internal class actions_animation
{
	private object[] properties;

	internal scenes_container container
	{
		get
		{
			return properties[0] as scenes_container;
		}
	}

	internal List<string> actions
	{
		get
		{
			return (properties[0] as scenes_container).actions;
		}
	}

	internal List<string> emotions
	{
		get
		{
			return (properties[0] as scenes_container).emotions;
		}
	}

	internal momiji_animation animation
	{
		get
		{
			return properties[1] as momiji_animation;
		}
	}

	internal string current1
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

	internal string current2
	{
		get
		{
			return properties[3] as string;
		}
		set
		{
			properties[3] = value;
		}
	}

	internal int frame1
	{
		get
		{
			return (int)properties[4];
		}
		set
		{
			properties[4] = value;
		}
	}

	internal int frame2
	{
		get
		{
			return (int)properties[5];
		}
		set
		{
			properties[5] = value;
		}
	}

	internal int ms1
	{
		get
		{
			return (int)properties[6];
		}
		set
		{
			properties[6] = value;
		}
	}

	internal int ms2
	{
		get
		{
			return (int)properties[7];
		}
		set
		{
			properties[7] = value;
		}
	}

	internal bool play1
	{
		get
		{
			return (bool)properties[8];
		}
		set
		{
			properties[8] = value;
		}
	}

	internal bool play2
	{
		get
		{
			return (bool)properties[9];
		}
		set
		{
			properties[9] = value;
		}
	}

	internal bool cover
	{
		get
		{
			return (bool)properties[10];
		}
		set
		{
			properties[10] = value;
		}
	}

	internal bool shadow
	{
		get
		{
			return (bool)properties[11];
		}
		set
		{
			properties[11] = value;
		}
	}

	internal bool elf
	{
		get
		{
			return (bool)properties[12];
		}
		set
		{
			properties[12] = value;
		}
	}

	internal string weapon
	{
		get
		{
			return properties[13] as string;
		}
		set
		{
			properties[13] = value;
		}
	}

	internal int delay1
	{
		get
		{
			return animation.delays1[frame1];
		}
	}

	internal int delay2
	{
		get
		{
			return animation.delays2[frame2];
		}
	}

	internal int count1
	{
		get
		{
			return null == animation ? 0 : animation[0].Count;
		}
	}

	internal int count2
	{
		get
		{
			return null == animation ? 0 : animation.Count;
		}
	}

	internal int width
	{
		get
		{
			return animation.width;
		}
	}

	internal int height
	{
		get
		{
			return animation.height;
		}
	}

	internal int x
	{
		get
		{
			return animation.originx;
		}
	}

	internal int y
	{
		get
		{
			return animation.originy;
		}
	}

	internal Bitmap frame
	{
		get
		{
			return null == animation ? null : animation[frame2][frame1];
		}
	}

	internal actions_animation(scenes_container container, bool play1, bool play2, bool cover, bool shadow, bool elf, string weapon)
	{
		properties = new object[]
		{
			container,
			null,
			"",
			"",
			0,
			0,
			0,
			0,
			play1,
			play2,
			cover,
			shadow,
			elf,
			weapon,
		};
	}

	internal void dispose()
	{
		if (null != animation)
			foreach (List<Bitmap> images in animation)
				foreach (Bitmap image in images)
					image.Dispose();
	}

	internal void update_animation()
	{
		bool fliped = null == animation ? false : animation.fliped;

		dispose();

		properties[1] = container.generate_animation(current1, current2, cover, shadow, elf, weapon);

		if (fliped)
			flip_animation();
	}

	internal void flip_animation()
	{
		if (null != animation)
			foreach (List<Bitmap> images in animation)
				foreach (Image image in images)
					image.RotateFlip(RotateFlipType.RotateNoneFlipX);

		animation.originx = animation.fliped ? -animation.originx - animation.width : -(animation.originx + animation.width);
		animation.fliped = !animation.fliped;
	}
}
