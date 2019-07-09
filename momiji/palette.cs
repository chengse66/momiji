using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal class palette : momiji_base
{
	private palette_wheel wheel;
	private palette_slider hue;
	private palette_slider saturation;
	private palette_slider value;
	private palette_slider red;
	private palette_slider green;
	private palette_slider blue;
	private palette_slider alpha;
	private palette_stack stack;
	private palette_indicator indicator;
	private Button accept;
	private Button cancel;

	private Color color_elder;

	internal Color color
	{
		get
		{
			return Color.FromArgb(alpha.value, red.value, green.value, blue.value);
		}
	}

	internal palette(Color color)
	{
		hsv hsv = hsv.rgb_2_hsv(color);

		wheel = new palette_wheel(hsv, 150);
		hue = new palette_slider((int)(360.0 * hsv.hue), 360, "H", 150, 6);
		saturation = new palette_slider((int)(100.0 * hsv.saturation), 100, "S", 150, 6);
		value = new palette_slider((int)(100.0 * hsv.value), 100, "V", 150, 6);
		red = new palette_slider(color.R, 255, "R", 150, 6);
		green = new palette_slider(color.G, 255, "G", 150, 6);
		blue = new palette_slider(color.B, 255, "B", 150, 6);
		alpha = new palette_slider(color.A, 255, "A", 150, 6);
		stack = new palette_stack(10);
		indicator = new palette_indicator(color, color);
		accept = new Button();
		cancel = new Button();

		wheel.Location = new Point(5, 5);
		wheel.Parent = this;

		wheel.value_changed += wheel_value_changed;

		hue.Location = new Point(160, 5);
		hue.Parent = this;

		hue.value_changed += hsv_value_changed;

		saturation.Location = new Point(160, 25);
		saturation.Parent = this;

		saturation.value_changed += hsv_value_changed;

		value.Location = new Point(160, 45);
		value.Parent = this;

		value.value_changed += hsv_value_changed;

		red.Location = new Point(160, 70);
		red.Parent = this;

		red.value_changed += rgb_value_changed;

		green.Location = new Point(160, 90);
		green.Parent = this;

		green.value_changed += rgb_value_changed;

		blue.Location = new Point(160, 110);
		blue.Parent = this;

		blue.value_changed += rgb_value_changed;

		alpha.Location = new Point(160, 130);
		alpha.Parent = this;

		alpha.value_changed += alpha_value_changed;

		stack.Location = new Point(5, 161);
		stack.Parent = this;

		stack.selected_index_changed += stack_selected_index_changed;

		indicator.Location = new Point(161, 161);
		indicator.Size = new Size(24, 14);
		indicator.Parent = this;

		indicator.restore += indicator_restore;

		accept.DialogResult = DialogResult.OK;
		accept.Location = new Point(235, 155);
		accept.Size = new Size(55, 20);
		accept.Parent = this;

		cancel.DialogResult = DialogResult.Cancel;
		cancel.Location = new Point(295, 155);
		cancel.Size = new Size(55, 20);
		cancel.Parent = this;

		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		ClientSize = new Size(355, 180);
		AcceptButton = accept;
		CancelButton = cancel;

		FormClosing += form_closing;

		color_elder = color;

		hue.update_gradient(new Color[] { Color.FromArgb(255, 0, 0), Color.FromArgb(255, 255, 0), Color.FromArgb(0, 255, 0), Color.FromArgb(0, 255, 255), Color.FromArgb(0, 0, 255), Color.FromArgb(255, 0, 255), Color.FromArgb(255, 0, 0), });
		saturation.update_gradient(new Color[] { hsv.hsv_2_rgb(hue.value, 0, value.value), hsv.hsv_2_rgb(hue.value, 100, value.value), });
		value.update_gradient(new Color[] { hsv.hsv_2_rgb(hue.value, saturation.value, 0), hsv.hsv_2_rgb(hue.value, saturation.value, 100), });

		red.update_gradient(new Color[] { Color.FromArgb(0, color.G, color.B), Color.FromArgb(255, color.G, color.B), });
		green.update_gradient(new Color[] { Color.FromArgb(color.R, 0, color.B), Color.FromArgb(color.R, 255, color.B), });
		blue.update_gradient(new Color[] { Color.FromArgb(color.R, color.G, 0), Color.FromArgb(color.R, color.G, 255), });
		alpha.update_gradient(new Color[] { Color.FromArgb(0, color.R, color.G, color.B), Color.FromArgb(255, color.R, color.G, color.B), });

		switch_language();
	}

	internal override void switch_language()
	{
		accept.Text = momiji_languages.palette_accept;
		cancel.Text = momiji_languages.palette_cancel;

		update_caption();

		switch_font();
	}

	private void update_caption()
	{
		Text = momiji_languages.palette_caption + " - " + string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", red.value, green.value, blue.value, alpha.value);
	}

	private void update_parameters(hsv hsv, Color color)
	{
		hue.value = (int)(360.0 * hsv.hue);
		saturation.value = (int)(100.0 * hsv.saturation);
		value.value = (int)(100.0 * hsv.value);

		saturation.update_gradient(new Color[] { hsv.hsv_2_rgb(hue.value, 0, value.value), hsv.hsv_2_rgb(hue.value, 100, value.value), });
		value.update_gradient(new Color[] { hsv.hsv_2_rgb(hue.value, saturation.value, 0), hsv.hsv_2_rgb(hue.value, saturation.value, 100), });

		red.value = color.R;
		green.value = color.G;
		blue.value = color.B;
		alpha.value = color.A;

		red.update_gradient(new Color[] { Color.FromArgb(0, color.G, color.B), Color.FromArgb(255, color.G, color.B), });
		green.update_gradient(new Color[] { Color.FromArgb(color.R, 0, color.B), Color.FromArgb(color.R, 255, color.B), });
		blue.update_gradient(new Color[] { Color.FromArgb(color.R, color.G, 0), Color.FromArgb(color.R, color.G, 255), });
		alpha.update_gradient(new Color[] { Color.FromArgb(0, color.R, color.G, color.B), Color.FromArgb(255, color.R, color.G, color.B), });

		indicator.color_current = color;

		update_caption();
	}

	private void wheel_value_changed()
	{
		hsv hsv = wheel.hsv;

		update_parameters(hsv, Color.FromArgb(alpha.value, hsv.hsv_2_rgb(hsv.hue, hsv.saturation, hsv.value)));
	}

	private void hsv_value_changed(object o)
	{
		Color color = Color.FromArgb(alpha.value, hsv.hsv_2_rgb(hue.value, saturation.value, value.value));

		wheel.hsv = new hsv(hue.value / 360.0, saturation.value / 100.0, value.value / 100.0);

		if (saturation != o)
			saturation.update_gradient(new Color[] { hsv.hsv_2_rgb(hue.value, 0, value.value), hsv.hsv_2_rgb(hue.value, 100, value.value), });
		if (value != o)
			value.update_gradient(new Color[] { hsv.hsv_2_rgb(hue.value, saturation.value, 0), hsv.hsv_2_rgb(hue.value, saturation.value, 100), });

		red.value = color.R;
		green.value = color.G;
		blue.value = color.B;

		red.update_gradient(new Color[] { Color.FromArgb(0, color.G, color.B), Color.FromArgb(255, color.G, color.B), });
		green.update_gradient(new Color[] { Color.FromArgb(color.R, 0, color.B), Color.FromArgb(color.R, 255, color.B), });
		blue.update_gradient(new Color[] { Color.FromArgb(color.R, color.G, 0), Color.FromArgb(color.R, color.G, 255), });
		alpha.update_gradient(new Color[] { Color.FromArgb(0, color.R, color.G, color.B), Color.FromArgb(255, color.R, color.G, color.B), });

		indicator.color_current = color;

		update_caption();
	}

	private void rgb_value_changed(object o)
	{
		Color color = Color.FromArgb(alpha.value, red.value, green.value, blue.value);
		hsv hsv = hsv.rgb_2_hsv(color);

		wheel.hsv = hsv;

		hue.value = (int)(360.0 * hsv.hue);
		saturation.value = (int)(100.0 * hsv.saturation);
		value.value = (int)(100.0 * hsv.value);

		saturation.update_gradient(new Color[] { hsv.hsv_2_rgb(hue.value, 0, value.value), hsv.hsv_2_rgb(hue.value, 100, value.value), });
		value.update_gradient(new Color[] { hsv.hsv_2_rgb(hue.value, saturation.value, 0), hsv.hsv_2_rgb(hue.value, saturation.value, 100), });

		if (red != o)
			red.update_gradient(new Color[] { Color.FromArgb(0, green.value, blue.value), Color.FromArgb(255, green.value, blue.value), });
		if (green != o)
			green.update_gradient(new Color[] { Color.FromArgb(red.value, 0, blue.value), Color.FromArgb(red.value, 255, blue.value), });
		if (blue != o)
			blue.update_gradient(new Color[] { Color.FromArgb(red.value, green.value, 0), Color.FromArgb(red.value, green.value, 255), });

		alpha.update_gradient(new Color[] { Color.FromArgb(0, red.value, green.value, blue.value), Color.FromArgb(255, red.value, green.value, blue.value), });

		indicator.color_current = color;

		update_caption();
	}

	private void alpha_value_changed(object o)
	{
		indicator.color_current = Color.FromArgb(alpha.value, red.value, green.value, blue.value);

		update_caption();
	}

	private void stack_selected_index_changed(int index, MouseButtons button)
	{
		if (MouseButtons.Left == button)
			indicator_restore(stack[index]);
		else if (MouseButtons.Right == button)
			stack[index] = Color.FromArgb(alpha.value, red.value, green.value, blue.value);

		update_caption();
	}

	private void indicator_restore(Color color)
	{
		hsv hsv = hsv.rgb_2_hsv(color);

		wheel.hsv = hsv;

		update_parameters(hsv, color);
	}

	private void form_closing(object o, FormClosingEventArgs e)
	{
		if (DialogResult.OK == this.DialogResult)
		{
			string[] colors = new string[stack.count];

			for (int index = 0; index < stack.count; ++index)
				colors[index] = stack[index].ToArgb().ToString();

			momiji_preferences.palette_default_colors = colors;
		}
	}

	private static void draw_border(bool hovered, Rectangle rectangle, Graphics graphics)
	{
		Pen pen = new Pen(hovered ? Color.FromArgb(0xd7, 0x8c, 0x54) : Color.FromArgb(0xa7, 0xa7, 0xa7));

		graphics.DrawLine(pen, rectangle.X + 1, rectangle.Y, rectangle.X + rectangle.Width - 2, rectangle.Y);
		graphics.DrawLine(pen, rectangle.X + 1, rectangle.Y + rectangle.Height - 1, rectangle.X + rectangle.Width - 2, rectangle.Y + rectangle.Height - 1);
		graphics.DrawLine(pen, rectangle.X, rectangle.Y + 1, rectangle.X, rectangle.Y + rectangle.Height - 2);
		graphics.DrawLine(pen, rectangle.X + rectangle.Width - 1, rectangle.Y + 1, rectangle.X + rectangle.Width - 1, rectangle.Y + rectangle.Height - 2);
	}

	private static TextureBrush generate_pattern_brush(int size)
	{
		Image image = generate_pattern(size);
		TextureBrush brush = new TextureBrush(image);

		image.Dispose();

		return brush;
	}

	private class hsv
	{
		internal static hsv rgb_2_hsv(Color color)
		{
			double hue, red, green, blue, minimum, maximum, drop;

			red = color.R / 255.0;
			green = color.G / 255.0;
			blue = color.B / 255.0;

			minimum = Math.Min(red, Math.Min(green, blue));
			maximum = Math.Max(red, Math.Max(green, blue));
			drop = maximum - minimum;

			if (.0 != drop)
			{
				if (red == maximum)
					hue = (green - blue) / drop;
				else if (green == maximum)
					hue = (blue - red) / drop + 2.0;
				else // if (blue == maximum)
					hue = (red - green) / drop + 4.0;

				if (.0 > hue)
					hue = hue + 6.0;

				return new hsv(hue / 6.0, drop / maximum, maximum);
			}

			return new hsv(.0, .0, maximum);
		}

		internal static Color hsv_2_rgb(int hue, int saturation, int value)
		{
			return hsv_2_rgb(hue / 360.0, saturation / 100.0, value / 100.0);
		}

		internal static Color hsv_2_rgb(double hue, double saturation, double value)
		{
			double f, p, q, t;

			if (.0 < saturation)
			{
				hue = 6.0 * hue;

				if (6.0 == hue)
					hue = .0;

				f = hue - Math.Floor(hue);
				p = value * (1.0 - saturation);
				q = value * (1.0 - saturation * f);
				t = value * (1.0 - saturation * (1.0 - f));

				switch ((int)Math.Floor(hue))
				{
					case 0: hue = value; saturation = t; value = p; break;

					case 1: hue = q; saturation = value; value = p; break;

					case 2: hue = p; saturation = value; value = t; break;

					case 3: hue = p; saturation = q;/* value = value;*/ break;

					case 4: hue = t; saturation = p;/* value = value; */break;

					case 5: hue = value; saturation = p; value = q; break;

					default: hue = .0; saturation = .0; value = .0; break;
				}

				return Color.FromArgb((int)(255.0 * hue), (int)(255.0 * saturation), (int)(255.0 * value));
			}

			value = 255.0 * value;

			return Color.FromArgb((int)value, (int)value, (int)value);
		}

		private object[] properties;

		internal double hue
		{
			get
			{
				return (double)properties[0];
			}
			set
			{
				properties[0] = value;
			}
		}

		internal double saturation
		{
			get
			{
				return (double)properties[1];
			}
			set
			{
				properties[1] = value;
			}
		}

		internal double value
		{
			get
			{
				return (double)properties[2];
			}
			set
			{
				properties[2] = value;
			}
		}

		internal hsv(double hue, double saturation, double value)
		{
			properties = new object[]
			{
				hue,
				saturation,
				value,
			};
		}
	}

	private class palette_wheel : Control
	{
		[DllImport("gdi32.dll")]
		private extern static int Ellipse(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

		[DllImport("gdi32.dll")]
		private extern static IntPtr GetStockObject(int fnObject);

		[DllImport("gdi32.dll")]
		private extern static IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		[DllImport("gdi32.dll")]
		private extern static int SetROP2(IntPtr hdc, int fnDrawMode);

		internal delegate void value_changed_event_handler();

		internal event value_changed_event_handler value_changed;

		private object[] properties;

		internal hsv hsv
		{
			get
			{
				return (hsv)properties[0];
			}
			set
			{
				properties[0] = value;

				generate_gradient_triangle();

				Refresh();
			}
		}

		private Bitmap wheel
		{
			get
			{
				return properties[1] as Bitmap;
			}
		}

		private Bitmap triangle
		{
			get
			{
				return properties[2] as Bitmap;
			}
		}

		private int current
		{
			get
			{
				return (int)properties[3];
			}
			set
			{
				properties[3] = value;
			}
		}

		internal palette_wheel(hsv hsv, int size)
		{
			properties = new object[]
			{
				hsv,
				new Bitmap(size, size),
				new Bitmap(size - size / 5, size - size / 5),
				0,
			};

			DoubleBuffered = true;
			Size = new Size(size, size);

			MouseDown += mousedown;
			MouseMove += mousemove;
			MouseUp += mouseup;
			Paint += paint;

			generate_gradient_wheel();
			generate_gradient_triangle();
		}

		protected override void Dispose(bool disposing)
		{
			if (null != wheel)
				wheel.Dispose();

			if (null != triangle)
				triangle.Dispose();

			base.Dispose(disposing);
		}

		private void generate_gradient_wheel()
		{
			Color[] colors;
			Region region;
			PathGradientBrush brush;
			GraphicsPath path = new GraphicsPath();
			GraphicsPath ellipse = new GraphicsPath();
			Graphics graphics = Graphics.FromImage(wheel);
			int diameter = wheel.Width;
			int inscribe = diameter / 10;

			--diameter;

			path.AddEllipse(0, 0, diameter, diameter);
			ellipse.AddEllipse(inscribe, inscribe, diameter - inscribe * 2, diameter - inscribe * 2);

			path.Flatten();

			region = new Region(ellipse);
			brush = new PathGradientBrush(path);
			colors = new Color[path.PointCount];

			for (int index = 0; index < path.PointCount; ++index)
			{
				double angle = (double)index / path.PointCount + 0.375/*135.0 / 180.0*/;

				if (.0 > angle)
					angle = angle + 1.0;
				else if (1.0 <= angle)
					angle = angle - 1.0;

				colors[index] = hsv.hsv_2_rgb(angle, 1.0, 1.0);
			}

			brush.SurroundColors = colors;

			graphics.SetClip(region, CombineMode.Exclude);

			graphics.FillEllipse(brush, 0, 0, diameter, diameter);

			graphics.SmoothingMode = SmoothingMode.AntiAlias;

			graphics.DrawEllipse(SystemPens.Control, 0, 0, diameter, diameter);
			graphics.DrawPath(SystemPens.Control, ellipse);

			graphics.Dispose();
			region.Dispose();
			brush.Dispose();
			ellipse.Dispose();
			path.Dispose();
		}

		unsafe private void generate_gradient_triangle()
		{
			double vsh;
			Graphics graphics;
			PointF hue, saturation, value;
			BitmapData data = triangle.LockBits(new Rectangle(Point.Empty, triangle.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			int* pixels = (int*)data.Scan0;

			compute_triangle(out hue, out saturation, out value);

			vsh = vector_cross(value, saturation, hue);

			for (int y = 0; y < triangle.Height; ++y)
				for (int x = 0; x < triangle.Width; ++x)
				{
					Point point = new Point(x, y);

					*pixels++ = compute_color(vsh,
						vector_cross(value, saturation, point),
						vector_cross(hue, value, point),
						vector_cross(saturation, hue, point), false);
				}

			triangle.UnlockBits(data);

			graphics = Graphics.FromImage(triangle);

			graphics.SmoothingMode = SmoothingMode.AntiAlias;

			graphics.DrawPolygon(SystemPens.Control, new PointF[] { hue, saturation, value, });

			graphics.Dispose();
		}

		private void compute_triangle(out PointF hue, out PointF saturation, out PointF value)
		{
			double radius = triangle.Width / 2.0;

			hue = rotate_point(radius, .0, radius, 0);
			saturation = rotate_point(radius, .0, radius, -120);
			value = rotate_point(radius, .0, radius, -240);
		}

		private PointF rotate_point(double x, double y, double position, int degree)
		{
			double radian = hsv.hue * Math.PI * 2.0 + Math.PI * (degree - 135.0) / 180.0;
			double sin = Math.Sin(radian);
			double cos = Math.Cos(radian);

			return new PointF((float)(position + cos * x - sin * y), (float)(position + cos * y + sin * x));
		}

		private PointF compute_point()
		{
			PointF hue, saturation, value;
			double offset = (wheel.Width - triangle.Width) / 2.0;

			compute_triangle(out hue, out saturation, out value);

			return new PointF(
				(float)(offset + value.X + hsv.value * (saturation.X - value.X) + hsv.value * hsv.saturation * (hue.X - saturation.X)),
				(float)(offset + value.Y + hsv.value * (saturation.Y - value.Y) + hsv.value * hsv.saturation * (hue.Y - saturation.Y)));
		}

		private bool point_on_wheel(double x, double y)
		{
			double radius = (wheel.Width - 1.0) / 2.0;
			double inner = radius - wheel.Width / 10.0;

			x = x - radius;
			y = y - radius;
			x = x * x + y * y;

			return radius * radius >= x && inner * inner <= x;
		}

		private bool point_on_triangle(double x, double y)
		{
			PointF hue, saturation, value;
			double offset = (wheel.Width - triangle.Width) / 2.0;
			PointF point = new PointF((float)(x - offset), (float)(y - offset));

			compute_triangle(out hue, out saturation, out value);

			return
				.0 <= vector_cross(value, saturation, point) &&
				.0 <= vector_cross(hue, value, point) &&
				.0 <= vector_cross(saturation, hue, point);
		}

		private double vector_cross(PointF a, PointF b, PointF p) // a=pa,b=pb |a||b|sin<a,b>
		{
			return (a.X - p.X) * (b.Y - p.Y) - (a.Y - p.Y) * (b.X - p.X);
		}

		private double vector_dot(PointF a, PointF b, PointF p) // a=pa,b=pb |a||b|cos<a,b>
		{
			return (a.X - p.X) * (b.X - p.X) + (a.Y - p.Y) * (b.Y - p.Y);
		}

		private int compute_color(double x, double y)
		{
			double vsh, vsp, hvp, shp;
			double offset = (wheel.Width - triangle.Width) / 2.0;
			PointF hue, saturation, value, point = new PointF((float)(x - offset), (float)(y - offset));

			compute_triangle(out hue, out saturation, out value);

			vsh = vector_cross(value, saturation, hue);

			vsp = vector_cross(value, saturation, point);
			hvp = vector_cross(hue, value, point);
			shp = vector_cross(saturation, hue, point);

			if (.0 > vsp)
				point = closest_point(saturation, value, point);
			else if (.0 > hvp)
				point = closest_point(value, hue, point);
			else if (.0 > shp)
				point = closest_point(hue, saturation, point);
			else
				return compute_color(vsh, vsp, hvp, shp, false);

			return compute_color(vsh,
				vector_cross(value, saturation, point),
				vector_cross(hue, value, point),
				vector_cross(saturation, hue, point), true);
		}

		private int compute_color(double vsh, double vsp, double hvp, double shp, bool coerce)
		{
			if ((.0 <= vsp && .0 <= hvp && .0 <= shp) || coerce)
			{
				Color color = hsv.hsv_2_rgb(hsv.hue, 1.0, 1.0);

				hvp = 255.0 * hvp;/*
				shp = .0 * shp;*/

				return Color.FromArgb(255,
					(int)((color.R * vsp + hvp/* + shp*/) / vsh),
					(int)((color.G * vsp + hvp/* + shp*/) / vsh),
					(int)((color.B * vsp + hvp/* + shp*/) / vsh)).ToArgb();
			}

			return 0;
		}

		private PointF closest_point(PointF a, PointF b, PointF p)
		{
			double r = vector_dot(b, p, a) / vector_dot(b, b, a);

			if (.0 >= r)
				return a;
			else if (1.0 <= r)
				return b;

			return new PointF((float)(a.X + r * (b.X - a.X)), (float)(a.Y + r * (b.Y - a.Y)));
		}

		private void draw_ellipse(PointF point, IntPtr hdc)
		{
			int x = (int)(Math.Round(point.X) - 2.0);
			int y = (int)(Math.Round(point.Y) - 2.0);
			IntPtr brush2 = SelectObject(hdc, GetStockObject(5/* NULL_BRUSH */));

			SetROP2(hdc, 6/* R2_NOT */);

			Ellipse(hdc, x, y, x + 5, y + 5);
			Ellipse(hdc, x - 1, y - 1, x + 6, y + 6);
		}

		private void mousedown(object o, MouseEventArgs e)
		{
			if (MouseButtons.Left == e.Button)
			{
				if (point_on_wheel(e.X, e.Y))
					current = 1;
				else if (point_on_triangle(e.X, e.Y))
					current = 2;
				else
					return;

				mousemove(o, e);
			}
		}

		private void mousemove(object o, MouseEventArgs e)
		{
			if (MouseButtons.Left == e.Button)
			{
				if (1 == current)
				{
					double radius = (wheel.Width - 1.0) / 2.0;
					double hue = Math.Atan2(e.Y - radius, e.X - radius);

					if (.0 > hue)
						hue = hue + Math.PI * 2;

					hue = (hue + Math.PI * 135.0 / 180.0) / Math.PI / 2.0;

					hsv = new hsv(hue - Math.Floor(hue), hsv.saturation, hsv.value);
				}
				else if (2 == current)
				{
					hsv sv = hsv.rgb_2_hsv(Color.FromArgb(compute_color(e.X, e.Y)));

					hsv.saturation = sv.saturation;
					hsv.value = sv.value;

					Refresh();
				}
				else
				{
					return;
				}

				if (null != value_changed)
					value_changed();
			}
		}

		private void mouseup(object o, MouseEventArgs e)
		{
			current = 0;
		}

		private void paint(object o, PaintEventArgs e)
		{
			IntPtr hdc;
			Bitmap bitmap = new Bitmap(Width, Height);
			Graphics graphics = Graphics.FromImage(bitmap);

			graphics.DrawImage(wheel, PointF.Empty);
			graphics.DrawImage(triangle, (float)(wheel.Width / 10.0), (float)(wheel.Width / 10.0));

			graphics.Dispose();

			e.Graphics.DrawImage(bitmap, Point.Empty);

			hdc = e.Graphics.GetHdc();

			draw_ellipse(rotate_point((wheel.Width - 1.0) / 2.0 - wheel.Width / 20.0, .0, wheel.Width / 2.0, 0), hdc);
			draw_ellipse(compute_point(), hdc);

			e.Graphics.ReleaseHdc();

			bitmap.Dispose();
		}
	}

	private class palette_slider : Control
	{
		internal delegate void value_changed_event_handler(object o);

		internal event value_changed_event_handler value_changed;

		private object[] properties;

		internal int value
		{
			get
			{
				return (int)properties[0];
			}
			set
			{
				if (0 > value)
					value = 0;
				else if (maximum < value)
					value = maximum;

				properties[0] = value;

				Refresh();
			}
		}

		internal int maximum
		{
			get
			{
				return (int)properties[1];
			}
		}

		private string text
		{
			get
			{
				return properties[2] + ":" + value;
			}
		}

		internal Image image
		{
			get
			{
				return properties[3] as Image;
			}
		}

		private bool hovered
		{
			get
			{
				return (bool)properties[4];
			}
			set
			{
				properties[4] = value;

				paint(this, new PaintEventArgs(Graphics.FromHwnd(Handle), this.ClientRectangle));
			}
		}

		internal palette_slider(int value, int maximum, string prefix, int width, int height)
		{
			properties = new object[]
			{
				value,
				maximum,
				prefix,
				new Bitmap(width, height),
				false,
			};

			DoubleBuffered = true;
			Size = new Size(width + 40, height + 9);

			MouseDown += mouse_down;
			MouseEnter += mouse_enter;
			MouseLeave += mouse_leave;
			MouseMove += mouse_move;
			Paint += paint;
		}

		protected override void Dispose(bool disposing)
		{
			if (null != image)
				image.Dispose();

			base.Dispose(disposing);
		}

		private void update_gradient_front(Color[] colors)
		{
			float[] positions = new float[colors.Length];
			Graphics graphics = Graphics.FromImage(image);
			Rectangle rectangle = new Rectangle(Point.Empty, image.Size);
			LinearGradientBrush brush = new LinearGradientBrush(rectangle, Color.Transparent, Color.Transparent, .0f);
			ColorBlend blend = new ColorBlend();

			for (int index = 0; index < positions.Length; ++index)
				positions[index] = index / (positions.Length - 1.0f);

			blend.Colors = colors;
			blend.Positions = positions;

			brush.InterpolationColors = blend;

			graphics.FillRectangle(brush, rectangle);

			graphics.Dispose();
			brush.Dispose();

			Refresh();
		}

		internal void update_gradient(Color[] colors)
		{
			TextureBrush texture = generate_pattern_brush((Height - 9) / 2);
			Graphics graphics = Graphics.FromImage(image);

			graphics.FillRectangle(texture, new Rectangle(Point.Empty, image.Size));

			graphics.Dispose();
			texture.Dispose();

			update_gradient_front(colors);
		}

		private void mouse_down(object o, MouseEventArgs e)
		{
			if (MouseButtons.Left == e.Button)
			{
				mouse_move(o, e);
			}
			else if (MouseButtons.Right == e.Button)
			{
				++value;

				if (null != value_changed)
					value_changed(this);
			}
		}

		private void mouse_enter(object o, EventArgs e)
		{
			hovered = true;
		}

		private void mouse_leave(object o, EventArgs e)
		{
			hovered = false;
		}

		private void mouse_move(object o, MouseEventArgs e)
		{
			Point point;

			if (MouseButtons.Left == e.Button)
			{
				point = PointToClient(MousePosition);
				value = (int)(maximum * (point.X - 2) / (Width - 40.0));

				if (null != value_changed)
					value_changed(this);

				Refresh();
			}
		}

		private void paint(object o, PaintEventArgs e)
		{
			Pen triangle;
			Bitmap bitmap = new Bitmap(Width, Height);
			Graphics graphics = Graphics.FromImage(bitmap);
			int position = (int)(value * (Width - 41.0) / maximum);

			graphics.Clear(BackColor);

			draw_border(hovered, new Rectangle(1, 0, Width - 36, Height - 5), graphics);

			if (hovered)
				triangle = new Pen(Color.FromArgb(0xc3, 0x76, 0x3d));
			else
				triangle = new Pen(Color.FromArgb(0x73, 0x73, 0x73));

			graphics.DrawPolygon(triangle, new Point[] { new Point(position + 3, Height - 5), new Point(position + 6, Height - 2), new Point(position + 6, Height - 1), new Point(position, Height - 1), new Point(position, Height - 2), });

			graphics.DrawString(text, Font, SystemBrushes.ControlText, Width - 35, 0);

			if (null != image)
				graphics.DrawImage(image, 3, 2, Width - 40, Height - 9);

			graphics.Dispose();
			triangle.Dispose();

			e.Graphics.DrawImage(bitmap, 0, 0);

			bitmap.Dispose();
		}
	}

	private class palette_stack : Control
	{
		private const int max_count = 14;

		internal delegate void selected_index_changed_event_handler(int index, MouseButtons button);

		internal event selected_index_changed_event_handler selected_index_changed;

		private object[] properties;

		private Color[] colors
		{
			get
			{
				return properties[0] as Color[];
			}
		}

		private bool hovered
		{
			get
			{
				return (bool)properties[1];
			}
			set
			{
				properties[1] = value;

				paint(this, new PaintEventArgs(Graphics.FromHwnd(Handle), this.ClientRectangle));
			}
		}

		internal int count
		{
			get
			{
				return colors.Length;
			}
		}

		internal Color this[int index]
		{
			get
			{
				return colors[index];
			}
			set
			{
				colors[index] = value;

				Refresh();
			}
		}

		internal palette_stack(int size)
		{
			Color[] defaults = new Color[max_count]
			{
				Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Purple,
				Color.Black, Color.DimGray, Color.Gray, Color.DarkGray, Color.LightGray, Color.White, Color.Transparent,
			};
			string[] colors = momiji_preferences.palette_default_colors;

			for (int index = 0, value; index < max_count && index < colors.Length; ++index)
				if (int.TryParse(colors[index], out value))
					defaults[index] = Color.FromArgb(value);

			properties = new object[]
			{
				defaults,
				false,
			};

			DoubleBuffered = true;
			Size = new Size(size * max_count + 4, size + 4);

			MouseDown += mouse_down;
			MouseEnter += mouse_enter;
			MouseLeave += mouse_leave;
			Paint += paint;
		}

		private void mouse_down(object o, MouseEventArgs e)
		{
			if (MouseButtons.Left == e.Button || MouseButtons.Right == e.Button)
				if (null != selected_index_changed)
					if (1 < e.X && Width - 2 > e.X && 1 < e.Y && Height - 2 > e.Y)
						selected_index_changed((e.X - 4) / (Height - 4), e.Button);
		}

		private void mouse_enter(object o, EventArgs e)
		{
			hovered = true;
		}

		private void mouse_leave(object o, EventArgs e)
		{
			hovered = false;
		}

		private void paint(object o, PaintEventArgs e)
		{
			int size = Height - 4;
			TextureBrush texture = generate_pattern_brush(size / 2);
			Bitmap bitmap = new Bitmap(Width, Height);
			Graphics graphics = Graphics.FromImage(bitmap);

			draw_border(hovered, new Rectangle(0, 0, Width, Height), e.Graphics);

			texture.TranslateTransform(2.0f, 2.0f);

			graphics.FillRectangle(texture, 2, 2, Width - 4, size);

			for (int index = 0; index < count; ++index)
			{
				SolidBrush brush = new SolidBrush(colors[index]);

				graphics.FillRectangle(brush, 2 + size * index, 2, size, size);

				brush.Dispose();
			}

			graphics.Dispose();
			texture.Dispose();

			e.Graphics.DrawImage(bitmap, 0, 0);

			bitmap.Dispose();
		}
	}

	private class palette_indicator : Control
	{
		internal delegate void restore_event_handler(Color color);

		internal event restore_event_handler restore;

		private object[] properties;

		internal Color color_elder
		{
			private get
			{
				return (Color)properties[0];
			}
			set
			{
				properties[0] = value;

				Refresh();
			}
		}

		internal Color color_current
		{
			private get
			{
				return (Color)properties[1];
			}
			set
			{
				properties[1] = value;

				Refresh();
			}
		}

		private bool hovered
		{
			get
			{
				return (bool)properties[2];
			}
			set
			{
				properties[2] = value;

				paint(this, new PaintEventArgs(Graphics.FromHwnd(Handle), this.ClientRectangle));
			}
		}

		internal palette_indicator(Color elder, Color current)
		{
			properties = new object[]
			{
				elder,
				current,
				false,
			};

			DoubleBuffered = true;

			MouseDown += mouse_down;
			MouseEnter += mouse_enter;
			MouseLeave += mouse_leave;
			Paint += paint;
		}

		private void mouse_down(object o, MouseEventArgs e)
		{
			if (MouseButtons.Left == e.Button)
				if (1 < e.X && 1 < e.Y && Width / 2 > e.X && Height - 2 > e.Y)
					if (null != restore)
						restore(color_elder);
		}

		private void mouse_enter(object o, EventArgs e)
		{
			hovered = true;
		}

		private void mouse_leave(object o, EventArgs e)
		{
			hovered = false;
		}

		private void paint(object o, PaintEventArgs e)
		{
			int size = Height - 4;
			TextureBrush texture = generate_pattern_brush(size / 2);
			Brush elder = new SolidBrush(color_elder);
			Brush current = new SolidBrush(color_current);
			Bitmap bitmap = new Bitmap(Width, Height);
			Graphics graphics = Graphics.FromImage(bitmap);

			draw_border(hovered, new Rectangle(0, 0, Width, Height), e.Graphics);

			texture.TranslateTransform(2.0f, 2.0f);

			graphics.FillRectangle(texture, 2, 2, Width - 4, size);

			graphics.FillRectangle(elder, 2, 2, size, size);
			graphics.FillRectangle(current, size + 2, 2, size, size);

			graphics.Dispose();
			texture.Dispose();
			elder.Dispose();
			current.Dispose();

			e.Graphics.DrawImage(bitmap, 0, 0);

			bitmap.Dispose();
		}
	}
}
