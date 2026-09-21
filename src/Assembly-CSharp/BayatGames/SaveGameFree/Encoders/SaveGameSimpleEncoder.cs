using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BayatGames.SaveGameFree.Encoders;

public class SaveGameSimpleEncoder : ISaveGameEncoder
{
	private const int Keysize = 256;

	private const int DerivationIterations = 1000;

	public string Encode(string input, string password)
	{
		byte[] array = Generate256BitsOfRandomEntropy();
		byte[] array2 = Generate256BitsOfRandomEntropy();
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		byte[] bytes2 = new Rfc2898DeriveBytes(password, array, 1000).GetBytes(32);
		using RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.BlockSize = 256;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.Padding = PaddingMode.PKCS7;
		using ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes2, array2);
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		byte[] inArray = array.Concat(array2).ToArray().Concat(memoryStream.ToArray())
			.ToArray();
		memoryStream.Close();
		cryptoStream.Close();
		return Convert.ToBase64String(inArray);
	}

	public string Decode(string input, string password)
	{
		byte[] array = Convert.FromBase64String(input);
		byte[] salt = array.Take(32).ToArray();
		byte[] rgbIV = array.Skip(32).Take(32).ToArray();
		byte[] array2 = array.Skip(64).Take(array.Length - 64).ToArray();
		byte[] bytes = new Rfc2898DeriveBytes(password, salt, 1000).GetBytes(32);
		using RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.BlockSize = 256;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.Padding = PaddingMode.PKCS7;
		using ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes, rgbIV);
		using MemoryStream memoryStream = new MemoryStream(array2);
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
		byte[] array3 = new byte[array2.Length];
		int count = cryptoStream.Read(array3, 0, array3.Length);
		memoryStream.Close();
		cryptoStream.Close();
		return Encoding.UTF8.GetString(array3, 0, count);
	}

	private static byte[] Generate256BitsOfRandomEntropy()
	{
		byte[] array = new byte[32];
		new RNGCryptoServiceProvider().GetBytes(array);
		return array;
	}
}
