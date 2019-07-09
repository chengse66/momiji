using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

internal class gif_encoder
{
	private IntPtr encoder_pointer;

	[DllImport("libgif.dll", EntryPoint = "#1", CharSet = CharSet.Unicode)]
	private extern static IntPtr construct(string location, int width, int height, int max_color, int back_color);

	[StructLayout(LayoutKind.Sequential)]
	private struct gif_encoder_structure
	{
		internal gif_encoder_destruct destruct;
		internal gif_encoder_append_frame append_frame;

		internal delegate void gif_encoder_destruct(IntPtr encoder_pointer);
		internal delegate void gif_encoder_append_frame(IntPtr pixels, int delay, IntPtr encoder_pointer);
	}

	private gif_encoder_structure encoder
	{
		get
		{
			return (gif_encoder_structure)Marshal.PtrToStructure(encoder_pointer, typeof(gif_encoder_structure));
		}
	}

	internal gif_encoder(string location, int width, int height, int max_color, Color back_color)
	{
		encoder_pointer = construct(location, width, height, max_color, back_color.ToArgb());
	}

	internal void destruct()
	{
		encoder.destruct(encoder_pointer);
	}

	internal void append_frame(Bitmap image, int delay)
	{
		BitmapData data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

		encoder.append_frame(data.Scan0, delay, encoder_pointer);

		image.UnlockBits(data);
	}
}
