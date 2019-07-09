using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Resources;
using System.ComponentModel;

internal class momiji_owned_base : momiji_base
{
	[DllImport("user32.dll")]
	protected extern static int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
	private extern static int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

	protected Point position;

	internal static Image warning
	{
		get
		{
			return query_image("warning");
		}
	}

	internal momiji_owned_base()
	{
		Move += move;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected void enable_explorer_style(Control control, bool enabled)
	{
		SetWindowTheme(control.Handle, enabled ? "explorer" : "", null);
	}

	private void move(object o, EventArgs e)
	{
		if (null != Owner)
			position = new Point(Left - Owner.Left, Top - Owner.Top);
	}

	internal void shift()
	{
		if (null != Owner)
			Location = new Point(Owner.Left + position.X, Owner.Top + position.Y);
	}

	protected class momiji_listview : ListView
	{
		internal momiji_listview()
		{
			DoubleBuffered = true;
		}
	}

	protected class momiji_tabcontrol : TabControl
	{
		[DllImport("user32.dll")]
		private extern static int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		private extern static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		internal int selected_index
		{
			set
			{
				SendMessage(Handle, 0x1300 + 12/* TCM_SETCURSEL = TCM_FIRST + 12 */, value, 0);

				OnSelectedIndexChanged(null);
			}
		}

		internal momiji_tabcontrol()
		{
		//	SetWindowLong(Handle, -16/* GWL_STYLE */, GetWindowLong(Handle, -16/* GWL_STYLE */) | 0x8000/* TCS_FOCUSNEVER */);

			MouseWheel += mouse_wheel;
		}

		private void mouse_wheel(object o, MouseEventArgs e)
		{
			int index = SelectedIndex - e.Delta / 120;

			if (0 <= index && TabPages.Count > index)
				SelectedIndex = index;
		}
	}
}

internal class momiji_base : Form
{
	protected Container container;

	protected static ResourceManager resource_manager
	{
		get
		{
			return new ResourceManager(typeof(momiji_host));
		}
	}

	protected Icon icon
	{
		get
		{
			return resource_manager.GetObject("icon") as Icon;
		}
	}

	protected momiji_base()
	{
		container = new Container();
	}

	protected static Image generate_pattern(int size)
	{
		Image image = new Bitmap(size * 2, size * 2);
		Graphics graphics = Graphics.FromImage(image);

		graphics.FillRectangles(Brushes.LightGray, new Rectangle[]
				{
					new Rectangle(0, 0, size, size),
					new Rectangle(size, size, size, size),
				});

		graphics.FillRectangles(Brushes.White, new Rectangle[]
				{
					new Rectangle(0, size, size, size),
					new Rectangle(size, 0, size, size),
				});

		graphics.Dispose();

		return image;
	}

	protected static Image query_image(string identity)
	{
		return resource_manager.GetObject(identity) as Image;
	}

	protected override void WndProc(ref Message m)
	{
		if (0x0006/* WM_ACTIVATE */ == m.Msg)
			if (IntPtr.Zero/* WA_INACTIVE */ == m.WParam)
			{
				m.Msg = 0x0086/* WM_NCACTIVATE */;
				m.WParam = (IntPtr)1/* TRUE */;
			}

		base.WndProc(ref m);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			container.Dispose();

		base.Dispose(disposing);
	}

	internal virtual void switch_language()
	{
	}

	internal virtual void update_preferences()
	{
	}

	protected void switch_font()
	{
		switch_font(this, new Font(momiji_languages.font_family, int.Parse(momiji_languages.font_size)));
	}

	private void switch_font(Control host, Font font)
	{
		foreach (Control control in host.Controls)
			switch_font(control, font);

		if (null != host.ContextMenuStrip)
			host.ContextMenuStrip.Font = font;

		host.Font = font;
	}

	protected class momiji_toolstrip : ToolStrip
	{
		protected override void WndProc(ref Message m)
		{
			if (0x0021/* WM_MOUSEACTIVATE */ == m.Msg)
				m.Result = (IntPtr)1/* MA_ACTIVATE */;
			else
				base.WndProc(ref m);
		}
	}
}
