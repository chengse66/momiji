using System;
using System.Drawing;
using System.Windows.Forms;

internal class code : momiji_base
{
	private TextBox textbox;
	private Button analyze;
	private Button accept;

	internal code(string code)
	{
		textbox = new TextBox();
		analyze = new Button();
		accept = new Button();

		textbox.Multiline = true;
		textbox.ScrollBars = ScrollBars.Vertical;
		textbox.Location = new Point(5, 5);
		textbox.Size = new Size(190, 65);
		textbox.Text = code;
		textbox.Parent = this;

		textbox.TextChanged += textbox_text_changed;

		analyze.Enabled = (1 < code.Split(new char[] { ',', }, StringSplitOptions.RemoveEmptyEntries).Length);
		analyze.DialogResult = DialogResult.OK;
		analyze.Location = new Point(40, 75);
		analyze.Size = new Size(75, 20);
		analyze.Parent = this;

		accept.DialogResult = DialogResult.Cancel;
		accept.Location = new Point(120, 75);
		accept.Size = new Size(75, 20);
		accept.Parent = this;

		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		ClientSize = new Size(200, 100);
		AcceptButton = analyze;
		CancelButton = accept;

		switch_language();
	}

	internal override void switch_language()
	{
		analyze.Text = momiji_languages.code_analyze;
		accept.Text = momiji_languages.code_accept;

		Text = momiji_languages.code_caption;

		switch_font();
	}

	internal string show_dialog()
	{
		if (DialogResult.OK == ShowDialog())
			if (1 < textbox.Text.Split(new char[] { ',', }, StringSplitOptions.RemoveEmptyEntries).Length)
				return textbox.Text;

		return "";
	}

	private void textbox_text_changed(object o, EventArgs e)
	{
		analyze.Enabled = 1 < textbox.Text.Split(new char[] { ',', }, StringSplitOptions.RemoveEmptyEntries).Length;
	}
}
