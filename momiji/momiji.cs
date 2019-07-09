using System;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

internal class momiji_host : momiji_base
{
	[DllImport("kernel32.dll")]
	private extern static bool SetDllDirectory(string lpPathName);

	internal static string assembly
	{
		get
		{
			return Application.StartupPath + "\\" + "assembly";
		}
	}

	[STAThread]
	private static void Main(string[] arguments)
	{
		MethodInfo update = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
		MethodInfo funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);

		update.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", assembly + ";", });

		SetDllDirectory(assembly);

		momiji_preferences.initialize();
		momiji_languages.initialize();

		Application.EnableVisualStyles();
		Application.Run((0 < arguments.Length || !File.Exists(assembly + "\\" + "momiji" + ".components")) ? new manager() : new momiji_host() as Form);
	}

	private ToolStripButton item_components;
	private ToolStripButton item_actions;
	private ToolStripButton item_scenes;
	private ToolStripSeparator item_separator_scenes_preferences;
	private ToolStripButton item_preferences;
	private ToolStripButton item_about;

	private momiji_toolstrip toolstrip;
	private momiji_canva canva;

	private components_host components;
	private actions_host actions;
	private scenes_host scenes;

	private momiji_host()
	{
		item_components = new ToolStripButton(null, query_image("toolstrip_components"), toolstrip_owned_click);
		item_actions = new ToolStripButton(null, query_image("toolstrip_actions"), toolstrip_owned_click);
		item_scenes = new ToolStripButton(null, query_image("toolstrip_scenes"), toolstrip_owned_click);
		item_separator_scenes_preferences = new ToolStripSeparator();
		item_preferences = new ToolStripButton(null, query_image("toolstrip_preferences"), toolstrip_preferences_click);
		item_about = new ToolStripButton(null, query_image("toolstrip_about"), toolstrip_about_click);

		toolstrip = new momiji_toolstrip();
		canva = new momiji_canva();

		canva.BackColor = momiji_preferences.advance_display_backcolor;
		canva.Parent = this;

		canva.DoubleClick += canva_double_click;

		toolstrip.ImageScalingSize = new System.Drawing.Size(32, 32);
		toolstrip.Parent = this;

		toolstrip.Items.AddRange(new ToolStripItem[]
		{
			item_components,
			item_actions,
			item_scenes,
			item_separator_scenes_preferences,
			item_preferences,
			item_about,
		});
		/*
		item_components.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_actions.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_scenes.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_preferences.DisplayStyle = ToolStripItemDisplayStyle.Image;
		item_about.DisplayStyle = ToolStripItemDisplayStyle.Image;
		*/
		item_components.Checked = true;
		item_actions.Checked = true;
		item_scenes.Checked = true;

		TopMost = momiji_preferences.general_top_most;
		StartPosition = FormStartPosition.WindowsDefaultBounds;
		Icon = icon;
		Text = "纸娃娃中文版";

		Load += load;
		Move += move;

		switch_culture(momiji_preferences.general_application_language);

		components = new components_host(this);
		actions = new actions_host(this);
		scenes = new scenes_host(this);

		item_components.Tag = components;
		item_actions.Tag = actions;
		item_scenes.Tag = scenes;

		components.initial(scenes);
		scenes.initial(components, actions);
	}

	internal override void switch_language()
	{
		item_components.Text = momiji_languages.toolstrip_components;
		item_actions.Text = momiji_languages.toolstrip_actions;
		item_scenes.Text = momiji_languages.toolstrip_scenes;
		item_preferences.Text = momiji_languages.toolstrip_preferences;
		item_about.Text = momiji_languages.toolstrip_about;

		switch_font();
	}

	internal override void update_preferences()
	{
		if (momiji_preferences.tgeneral_top_most != momiji_preferences.general_top_most)
			TopMost = momiji_preferences.general_top_most;

		if (momiji_preferences.tadvance_display_backcolor != momiji_preferences.advance_display_backcolor)
			canva.BackColor = momiji_preferences.advance_display_backcolor;

		foreach (momiji_owned_base owned in OwnedForms)
			owned.update_preferences();
	}

	internal void update_ui_culture()
	{
		switch_culture(momiji_preferences.general_application_language);
	}

	internal void switch_culture(string ietf)
	{
		momiji_languages.alter(ietf);

		switch_language();

		foreach (momiji_base owned in OwnedForms)
			owned.switch_language();
	}

	internal void add_animation(actions_animation animation)
	{
		canva.add_animation(animation);
	}

	internal void delete_animation(int index)
	{
		canva.delete_animation(index);
	}

	internal void move_animation(int index, int delta)
	{
		canva.move_animation(index, delta);
	}

	internal void update_canva()
	{
		canva.Refresh();
	}

	private void scene_selected(actions_animation animation)
	{
		scenes.scene_selected(animation.container);
	}

	private void toolstrip_owned_click(object o, EventArgs e)
	{
		ToolStripButton button = o as ToolStripButton;
		momiji_owned_base owned = button.Tag as momiji_owned_base;

		button.Checked = !button.Checked;

		if (button.Checked)
			owned.Show();
		else
			owned.Hide();
	}

	private void toolstrip_preferences_click(object o, EventArgs e)
	{
		using (preferences preferences = new preferences())
			if (preferences.show_dialog(this))
			{
				update_preferences();

				momiji_preferences.apply();
				momiji_preferences.save();
			}
			else
			{
				switch_culture(momiji_preferences.general_application_language);
			}
	}

	private void toolstrip_about_click(object o, EventArgs e)
	{
		using (about about = new about())
			about.ShowDialog();
	}

	private void canva_double_click(object o, EventArgs e)
	{
		Point client = PointToScreen(Point.Empty);

		components.Location = new Point(client.X, client.Y + ClientSize.Height - components.Height);
		actions.Location = new Point(client.X + ClientSize.Width - actions.Width, client.Y + ClientSize.Height - actions.Height);
		scenes.Location = new Point(client.X + ClientSize.Width - scenes.Width, client.Y);
	}

	private void load(object o, EventArgs e)
	{
		components.show(this);
		actions.show(this);
		scenes.show(this);
	}

	private void move(object o, EventArgs e)
	{
		components.shift();
		actions.shift();
		scenes.shift();
	}

	internal void owned_form_closing(object o, FormClosingEventArgs e)
	{
		if (components == o)
			item_components.Checked = false;
		else if (actions == o)
			item_actions.Checked = false;
		else if (scenes == o)
			item_scenes.Checked = false;

		if (CloseReason.UserClosing == e.CloseReason)
		{
			(o as momiji_owned_base).Hide();

			e.Cancel = true;
		}
	}

	private class momiji_canva : Panel
	{
		private List<actions_animation> animations;
		private List<wzvector> locations;

		private int degree;

		private int current;

		private Point origin;

		internal momiji_canva()
		{
			DoubleBuffered = true;
			BorderStyle = BorderStyle.FixedSingle;
			Dock = DockStyle.Fill;
			BackColor = Color.LightGray;
			BackgroundImage = generate_pattern(8);

			MouseDown += mouse_down;
			MouseMove += mouse_move;
			MouseUp += mouse_up;
			Paint += paint;

			animations = new List<actions_animation>();
			locations = new List<wzvector>();
			current = -1;
			degree = -90;
		}

		internal void add_animation(actions_animation animation)
		{
			double radius = ClientSize.Height / 4.0;
			double radian = Math.PI * degree / 180.0;
			double sin = Math.Sin(radian);
			double cos = Math.Cos(radian);

			degree = degree + 25;

			animations.Add(animation);
			locations.Add(new wzvector((int)(ClientSize.Width / 2.0 + cos * radius), (int)(ClientSize.Height / 2.0 + sin * radius)));

			Refresh();
		}

		internal void delete_animation(int index)
		{
			animations.RemoveAt(index);
			locations.RemoveAt(index);

			Refresh();
		}

		internal void move_animation(int index, int delta)
		{
			int position = index + delta;

			if (0 <= position && animations.Count > position)
			{
				actions_animation animation = animations[index];
				wzvector location = locations[index];

				animations.RemoveAt(index);
				locations.RemoveAt(index);

				animations.Insert(position, animation);
				locations.Insert(position, location);

				Refresh();
			}
		}

		private void mouse_down(object o, MouseEventArgs e)
		{
			if (MouseButtons.Left == e.Button)
			{
				for (int index = 0; index < animations.Count; ++index)
				{
					actions_animation animation = animations[index];
					wzvector location = locations[index];
					Point point = new Point(e.X - location.x - animation.x, e.Y - location.y - animation.y);

					if (new Rectangle(Point.Empty, animation.frame.Size).Contains(point))
						if (0 != animation.frame.GetPixel(point.X, point.Y).A)
						{
							current = index;
							origin = new Point(e.X, e.Y);
						}
				}

				if (-1 != current)
					(Parent as momiji_host).scene_selected(animations[current]);
			}
		}

		private void mouse_move(object o, MouseEventArgs e)
		{
			if (-1 != current)
			{
				wzvector location = locations[current];

				locations[current] = new wzvector(location.x + e.X - origin.X, location.y + e.Y - origin.Y);

				origin = new Point(e.X, e.Y);

				Refresh();
			}
		}

		private void mouse_up(object o, MouseEventArgs e)
		{
			current = -1;
		}

		private void paint(object o, PaintEventArgs e)
		{
			Brush brush = new SolidBrush(BackColor);

			e.Graphics.FillRectangle(brush, ClientRectangle);

			brush.Dispose();

			for (int index = 0; index < animations.Count; ++index)
			{
				actions_animation animation = animations[index];

				if (null != animation.animation)
				{
					wzvector location = locations[index];

					e.Graphics.DrawImage(animation.frame, location.x + animation.x, location.y + animation.y);
				}
			}
		}
	}
}

internal class momiji_animation : List<List<Bitmap>>
{
	private object[] properties;

	internal int originx
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

	internal int originy
	{
		get
		{
			return (int)properties[1];
		}
		set
		{
			properties[1] = value;
		}
	}

	internal int width
	{
		get
		{
			return (int)properties[2];
		}
	}

	internal int height
	{
		get
		{
			return (int)properties[3];
		}
	}

	internal int[] delays1
	{
		get
		{
			return properties[4] as int[];
		}
	}

	internal int[] delays2
	{
		get
		{
			return properties[5] as int[];
		}
	}

	internal bool fliped
	{
		get
		{
			return (bool)properties[6];
		}
		set
		{
			properties[6] = value;
		}
	}

	internal momiji_animation(int x, int y, int width, int height, int[] delays1, int[] delays2)
	{
		properties = new object[]
		{
			x,
			y,
			width,
			height,
			delays1,
			delays2,
			false,
		};
	}
}
