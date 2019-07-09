using System.Drawing;
using System.Windows.Forms;

internal class about : momiji_base
{
	private PictureBox picturebox;
	private TextBox version;
	private TextBox description;
	private TextBox company;
	private TextBox copyright;
    private TextBox custom;
	internal about()
	{
		picturebox = new PictureBox();
		version = new TextBox();
		description = new TextBox();
		company = new TextBox();
		copyright = new TextBox();
        custom = new TextBox();


        picturebox.Location = new Point(5, 5);
		picturebox.Size = new Size(100, 100);
		picturebox.Image = query_image("portrait");
		picturebox.Parent = this;

		version.AutoSize = true;
		version.ReadOnly = true;
		version.BorderStyle = BorderStyle.None;
		version.Location = new Point(110, 5);
		version.Size = new Size(150, 20);
		version.Text = "momiji " + ProductVersion;
		version.Parent = this;

		description.AutoSize = true;
		description.ReadOnly = true;
		description.BorderStyle = BorderStyle.None;
		description.Location = new Point(110, 30);
		description.Size = new Size(150, 20);
		description.Parent = this;

		company.AutoSize = true;
		company.ReadOnly = true;
		company.BorderStyle = BorderStyle.None;
		company.Location = new Point(110, 55);
		company.Size = new Size(150, 20);
		company.Parent = this;

		copyright.AutoSize = true;
		copyright.ReadOnly = true;
		copyright.BorderStyle = BorderStyle.None;
		copyright.Location = new Point(110, 80);
		copyright.Size = new Size(150, 20);
		copyright.Parent = this;

        custom.AutoSize = true;
        custom.ReadOnly = true;
        custom.Multiline = true;
        custom.TextAlign = HorizontalAlignment.Center;
        custom.BorderStyle = BorderStyle.None;
        custom.Location = new Point(0, 120);
        custom.Size = new Size(265, 60);
        custom.Parent = this;



		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		ClientSize = new Size(265, 180);

		switch_language();
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		switch ((Keys)msg.WParam.ToInt32())
		{
			case Keys.Enter:

			case Keys.Escape: Close(); break;
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	internal override void switch_language()
	{
		description.Text = momiji_languages.about_description;
		company.Text = momiji_languages.about_company;
		copyright.Text = momiji_languages.about_copyright;
        custom.Text = "欢迎可爱的手动玩家\r\n加入顽皮兔Recuerdos小家庭。";
        Text = momiji_languages.about_caption;

		switch_font();
	}
}
