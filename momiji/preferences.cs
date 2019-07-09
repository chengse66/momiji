using System;
using System.Drawing;
using System.Windows.Forms;

internal class preferences : momiji_base
{
	private TabControl tabcontrol;
	private Button accept;
	private Button cancel;
	private CheckBox explorer_style;
	private CheckBox top_most;
	private Label application_language;
	private ComboBox combobox;
	private GroupBox advance_output;
	private Label advance_outputlocation;
	private TextBox advance_textbox;
	private CheckBox advance_outputframes;
	private TextBox advance_numeric;
	private Label advance_outputmaxcolor;
	private Button advance_outputbackcolor;
	private GroupBox advance_display;
	private Label advance_displayitems;
	private TextBox advance_count;
	private Button advance_displaybackcolor;

	internal preferences()
	{
		tabcontrol = new TabControl();
		accept = new Button();
		cancel = new Button();
		explorer_style = new CheckBox();
		top_most = new CheckBox();
		application_language = new Label();
		combobox = new ComboBox();
		advance_output = new GroupBox();
		advance_outputlocation = new Label();
		advance_textbox = new TextBox();
		advance_outputframes = new CheckBox();
		advance_numeric = new TextBox();
		advance_outputmaxcolor = new Label();
		advance_outputbackcolor = new Button();
		advance_display = new GroupBox();
		advance_displayitems = new Label();
		advance_count = new TextBox();
		advance_displaybackcolor = new Button();

		tabcontrol.Size = new Size(300, 145);
		tabcontrol.Parent = this;

		tabcontrol.TabPages.Add("");
		tabcontrol.TabPages.Add("");

		accept.DialogResult = DialogResult.OK;
		accept.Location = new Point(180, 150);
		accept.Size = new Size(55, 20);
		accept.Parent = this;

		cancel.DialogResult = DialogResult.Cancel;
		cancel.Location = new Point(240, 150);
		cancel.Size = new Size(55, 20);
		cancel.Parent = this;

		explorer_style.Checked = momiji_preferences.general_explorer_style;
		explorer_style.TextAlign = ContentAlignment.BottomLeft;
		explorer_style.Location = new Point(5, 5);
		explorer_style.Size = new Size(tabcontrol.DisplayRectangle.Width - 10, 20);
		explorer_style.Parent = tabcontrol.TabPages[0];

		top_most.Checked = momiji_preferences.general_top_most;
		top_most.TextAlign = ContentAlignment.BottomLeft;
		top_most.Location = new Point(5, 30);
		top_most.Size = new Size(tabcontrol.DisplayRectangle.Width - 10, 20);
		top_most.Parent = tabcontrol.TabPages[0];

		application_language.TextAlign = ContentAlignment.MiddleLeft;
		application_language.Location = new Point(5, 55);
		application_language.Size = new Size(55, 20);
		application_language.Parent = tabcontrol.TabPages[0];

		combobox.DropDownStyle = ComboBoxStyle.DropDownList;
		combobox.Location = new Point(65, 55);
		combobox.Size = new Size(tabcontrol.DisplayRectangle.Width - 70, 100);
		combobox.Parent = tabcontrol.TabPages[0];

		combobox.Items.AddRange(momiji_languages.names);

		combobox.SelectedIndex = momiji_languages.index_of(momiji_preferences.general_application_language);

		combobox.SelectedIndexChanged += combobox_selected_index_changed;

		advance_output.Location = new Point(5, 5);
		advance_output.Size = new Size(tabcontrol.DisplayRectangle.Width - 10, 65);
		advance_output.Parent = tabcontrol.TabPages[1];

		advance_outputlocation.TextAlign = ContentAlignment.MiddleLeft;
		advance_outputlocation.Location = new Point(5, 15);
		advance_outputlocation.Size = new Size(55, 20);
		advance_outputlocation.Parent = advance_output;

		advance_textbox.AutoSize = false;
		advance_textbox.HideSelection = false;
		advance_textbox.ReadOnly = true;
		advance_textbox.Location = new Point(65, 15);
		advance_textbox.Size = new Size(tabcontrol.DisplayRectangle.Width - 165, 20);
		advance_textbox.Text = momiji_preferences.advance_output_location;
		advance_textbox.Parent = advance_output;

		advance_textbox.Click += advance_textbox_click;

		advance_textbox.Select(0, advance_textbox.Text.Length);

		advance_numeric.AutoSize = false;
		advance_numeric.HideSelection = false;
		advance_numeric.Location = new Point(tabcontrol.DisplayRectangle.Width - 95, 15);
		advance_numeric.Size = new Size(35, 20);
		advance_numeric.Text = momiji_preferences.advance_output_maxcolor.ToString();
		advance_numeric.Parent = advance_output;

		advance_numeric.KeyPress += advance_numeric_key_press;
		advance_numeric.MouseWheel += advance_numeric_mouse_wheel;

		advance_outputmaxcolor.TextAlign = ContentAlignment.MiddleLeft;
		advance_outputmaxcolor.Location = new Point(tabcontrol.DisplayRectangle.Width - 55, 15);
		advance_outputmaxcolor.Size = new Size(40, 20);
		advance_outputmaxcolor.Parent = advance_output;

		advance_outputframes.Checked = momiji_preferences.advance_output_frames;
		advance_outputframes.TextAlign = ContentAlignment.BottomLeft;
		advance_outputframes.Location = new Point(5, 40);
		advance_outputframes.Size = new Size(165, 20);
		advance_outputframes.Parent = advance_output;

		advance_outputbackcolor.ImageAlign = ContentAlignment.MiddleLeft;
		advance_outputbackcolor.Location = new Point(175, 40);
		advance_outputbackcolor.Size = new Size(tabcontrol.DisplayRectangle.Width - 190, 20);
		advance_outputbackcolor.Tag = momiji_preferences.advance_output_backcolor;
		advance_outputbackcolor.Parent = advance_output;

		advance_outputbackcolor.Click += advance_outputcolor_click;

		advance_display.Location = new Point(5, 75);
		advance_display.Size = new Size(tabcontrol.DisplayRectangle.Width - 10, 40);
		advance_display.Parent = tabcontrol.TabPages[1];

		advance_displayitems.TextAlign = ContentAlignment.MiddleLeft;
		advance_displayitems.Location = new Point(5, 15);
		advance_displayitems.Size = new Size(125, 20);
		advance_displayitems.Parent = advance_display;

		advance_count.AutoSize = false;
		advance_count.HideSelection = false;
		advance_count.Location = new Point(135, 15);
		advance_count.Size = new Size(35, 20);
		advance_count.Text = momiji_preferences.advance_display_items.ToString();
		advance_count.Parent = advance_display;

		advance_count.KeyPress += advance_count_key_press;
		advance_count.MouseWheel += advance_count_mouse_wheel;

		advance_displaybackcolor.ImageAlign = ContentAlignment.MiddleLeft;
		advance_displaybackcolor.Location = new Point(175, 15);
		advance_displaybackcolor.Size = new Size(tabcontrol.DisplayRectangle.Width - 190, 20);
		advance_displaybackcolor.Tag = momiji_preferences.advance_display_backcolor;
		advance_displaybackcolor.Parent = advance_display;

		advance_displaybackcolor.Click += advance_backcolor_click;

		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		ClientSize = new Size(300, 175);
		AcceptButton = accept;
		CancelButton = cancel;

		update_image(advance_outputbackcolor);
		update_image(advance_displaybackcolor);

		switch_language();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (null != advance_outputbackcolor.Image)
				advance_outputbackcolor.Image.Dispose();

			if (null != advance_displaybackcolor.Image)
				advance_displaybackcolor.Image.Dispose();
		}

		base.Dispose(disposing);
	}

	internal override void switch_language()
	{
		tabcontrol.TabPages[0].Text = momiji_languages.preferences_general;
		tabcontrol.TabPages[1].Text = momiji_languages.preferences_advance;

		accept.Text = momiji_languages.preferences_accept;
		cancel.Text = momiji_languages.preferences_cancel;
		explorer_style.Text = momiji_languages.preferences_general_explorer_style;
		top_most.Text = momiji_languages.preferences_general_top_most;
		application_language.Text = momiji_languages.preferences_general_application_language;

		advance_output.Text = momiji_languages.preferences_advance_output;
		advance_outputlocation.Text = momiji_languages.preferences_advance_output_location;
		advance_outputframes.Text = momiji_languages.preferences_advance_output_frames;
		advance_outputmaxcolor.Text = momiji_languages.preferences_advance_output_maxcolor;
		advance_outputbackcolor.Text = momiji_languages.preferences_advance_output_backcolor;
		advance_display.Text = momiji_languages.preferences_advance_display;
		advance_displayitems.Text = momiji_languages.preferences_advance_display_count;
		advance_displaybackcolor.Text = momiji_languages.preferences_advance_display_backcolor;

		Text = momiji_languages.preferences_caption;

		switch_font();
	}

	internal bool show_dialog(momiji_host host)
	{
		if (DialogResult.OK == ShowDialog(host))
		{
			momiji_preferences.general_explorer_style = explorer_style.Checked;
			momiji_preferences.general_top_most = top_most.Checked;
			momiji_preferences.general_application_language = momiji_languages.ietf_at(combobox.SelectedIndex);
			momiji_preferences.advance_output_location = advance_textbox.Text;
			momiji_preferences.advance_output_frames = advance_outputframes.Checked;
			momiji_preferences.advance_output_maxcolor = receive_number(advance_numeric, 8, 256);
			momiji_preferences.advance_output_backcolor = (Color)advance_outputbackcolor.Tag;
			momiji_preferences.advance_display_items = receive_number(advance_count, 10, 100);
			momiji_preferences.advance_display_backcolor = (Color)advance_displaybackcolor.Tag;

			return true;
		}

		return false;
	}

	private int receive_number(TextBox textbox, int minimum, int maximum)
	{
		int value;

		if (!int.TryParse(textbox.Text, out value))
			return maximum;

		return minimum > value ? minimum : maximum < value ? maximum : value;
	}

	private void restrict_value(TextBox textbox, int minimum, int maximum)
	{
		int value;

		if (!int.TryParse(textbox.Text, out value))
			value = maximum + 1;

		if (minimum > value)
			textbox.Text = minimum.ToString();
		else if (maximum < value)
			textbox.Text = maximum.ToString();
	}

	private void select_color(Button button)
	{
		using (palette palette = new palette((Color)button.Tag))
			if (DialogResult.OK == palette.ShowDialog())
			{
				button.Tag = palette.color;

				update_image(button);
			}
			else
			{
				momiji_preferences.palette_default_colors = momiji_preferences.tpalette_default_colors;
			}
	}

	private void update_image(Button button)
	{
		Image image = generate_pattern(8);
		Graphics graphics = Graphics.FromImage(image);
		SolidBrush brush = new SolidBrush((Color)button.Tag);

		graphics.FillRectangle(brush, 0, 0, image.Width, image.Height);

		graphics.Dispose();

		if (null != button.Image)
			button.Image.Dispose();

		button.Image = image;
	}

	private void combobox_selected_index_changed(object o, EventArgs e)
	{
		(Owner as momiji_host).switch_culture(momiji_languages.ietf_at(combobox.SelectedIndex));
	}

	private void advance_textbox_click(object o, EventArgs e)
	{
		using (FolderBrowserDialog folder = new FolderBrowserDialog())
		{
			folder.Description = momiji_languages.preferences_advance_output_folderdialog;
			folder.SelectedPath = advance_textbox.Text;

			if (DialogResult.OK == folder.ShowDialog())
				advance_textbox.Text = folder.SelectedPath;
		}
	}

	private void advance_numeric_key_press(object o, KeyPressEventArgs e)
	{
		if (char.IsDigit(e.KeyChar))
		{
			advance_numeric.Paste(e.KeyChar.ToString());

			restrict_value(advance_numeric, 8, 256);

			e.Handled = true;
		}
		else if ('\b' != e.KeyChar)
		{
			e.Handled = true;
		}
	}

	private void advance_numeric_mouse_wheel(object o, MouseEventArgs e)
	{
		restrict_value(advance_numeric, 8, 256);

		advance_numeric.Text = (int.Parse(advance_numeric.Text) + e.Delta / 120).ToString();

		restrict_value(advance_numeric, 8, 256);
	}

	private void advance_outputcolor_click(object o, EventArgs e)
	{
		select_color(advance_outputbackcolor);
	}

	private void advance_count_key_press(object o, KeyPressEventArgs e)
	{
		if (char.IsDigit(e.KeyChar))
		{
			advance_count.Paste(e.KeyChar.ToString());

			restrict_value(advance_count, 10, 100);

			e.Handled = true;
		}
		else if ('\b' != e.KeyChar)
		{
			e.Handled = true;
		}
	}

	private void advance_count_mouse_wheel(object o, MouseEventArgs e)
	{
		restrict_value(advance_count, 10, 100);

		advance_count.Text = (int.Parse(advance_count.Text) + e.Delta / 120).ToString();

		restrict_value(advance_count, 10, 100);
	}

	private void advance_backcolor_click(object o, EventArgs e)
	{
		select_color(advance_displaybackcolor);
	}
}
