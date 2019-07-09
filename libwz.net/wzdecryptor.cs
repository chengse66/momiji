using System.IO;
using System.Text;
using System.Security.Cryptography;

internal class wzdecryptor
{
	private readonly static byte[] factork = { 0xB9, 0x7D, 0x63, 0xE9, };
	private readonly static byte[] factorg = { 0x4D, 0x23, 0xC7, 0x2B, };
	private readonly static byte[] key =
	{
		0x13, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00,
		0x06, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00,
		0x1B, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00,
		0x33, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00,
	};

	private byte[] cryptok;
	private byte[] cryptog;
	private byte[] cryptox;
	private byte[] cryptoc;

	internal char region
	{
		set
		{
			cryptoc = 'k' == value ? cryptok : 'g' == value ? cryptog : cryptox;
		}
	}

	protected wzdecryptor()
	{
		cryptok = generate_sequence(factork);
		cryptog = generate_sequence(factorg);
		cryptox = new byte[0xffff];
		cryptoc = cryptok;
	}

	private byte[] generate_sequence(byte[] factor)
	{
		using (Rijndael rijndael = Rijndael.Create())
		{
			rijndael.Key = key;
			rijndael.Mode = CipherMode.ECB;

			using (MemoryStream stream = new MemoryStream())
			{
				using (CryptoStream crypto = new CryptoStream(stream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
				{
					int size = 4 * factor.Length;
					byte[] transform = new byte[size];

					for (int index = 0; index < 4; ++index)
						factor.CopyTo(transform, factor.Length * index);

					for (int index = 0; index < 0x1000; ++index)
					{
						crypto.Write(transform, 0, size);
						stream.Seek(-size, SeekOrigin.Current);
						stream.Read(transform, 0, size);
					}

					stream.SetLength(0xffff);
				}

				return stream.ToArray();
			}
		}
	}

	protected string decrypt_string(byte[] bytes)
	{
		StringBuilder result = new StringBuilder();
		byte factor = 0xaa;

		for (int index = 0; index < bytes.Length; ++index, ++factor)
			result.Append((char)(bytes[index] ^ cryptoc[index] ^ factor));

		return result.ToString();
	}

	protected string decrypt_string16(byte[] bytes)
	{
		StringBuilder result = new StringBuilder();
		ushort factor = 0xaaaa;

		for (int index = 0; index < bytes.Length; index = index + 2, ++factor)
			result.Append((char)(((bytes[index + 1] ^ cryptoc[index + 1] ^ (factor >> 0x08)) << 0x08) + (bytes[index] ^ cryptoc[index] ^ (factor & 0xff))));

		return result.ToString();
	}

	internal byte[] decrypt_bytes(byte[] bytes)
	{
		for (int index = 0; index < bytes.Length; ++index)
			bytes[index] = (byte)(bytes[index] ^ cryptoc[index]);

		return bytes;
	}
}
