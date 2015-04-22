using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CalbucciLib.Tests
{
	public class TestCookie1 : SmartCookie
	{
		public TestCookie1(HttpContextBase context)
			:base(context)
		{ }

		public string Val1 { get { return GetValue(); } set { SetValue(value); }}
		public int Val2 { get { return GetValueInt(); } set { SetValue(value); }}
		public long Val3 { get { return GetValueLong(); } set { SetValue(value); } }
		public double Val4 { get { return GetValueDouble(); } set { SetValue(value); }}
		public DateTime? Val5 { get { return GetValueDateTime(); } set { SetValue(value); } }
		public bool Val6 { get { return GetValueBool(); } set { SetValue(value); } }
	}



	public class TestCookie2 : SmartCookie
	{
		public TestCookie2(HttpContextBase context)
			:base(context)
		{ }

		public TestCookie2SubValues Values1
		{
			get { return GetMultiValue<TestCookie2SubValues>(); }
		}

		public TestCookie2SubValues Values2
		{
			get { return GetMultiValue<TestCookie2SubValues>(); }
		}
			
	}

	public class TestCookie2SubValues : SmartCookieMultiValue
	{
		public string Val1 { get { return GetValue(); } set { SetValue(value); } }
		public int Val2 { get { return GetValueInt(); } set { SetValue(value); } }
	}


	public class TestCookie3 : SmartCookie
	{
		public TestCookie3(HttpContextBase context)
			: base(context)
		{ }

		[SmartCookieSettings("v1", 30, Domain = "domain.com", HttpOnly = true, MaxLength = 10, Path = "/a/", Secure = true)]
		public string Val1 { get { return GetValue(); } set { SetValue(value); } }

		[SmartCookieSettings("v2")]
		public TestCookie3SubValues MVal2
		{
			get { return GetMultiValue<TestCookie3SubValues>(); }
		}

	}

	public class TestCookie3SubValues : SmartCookieMultiValue
	{
		[SmartCookieSettings("v31", MaxLength = 8)]
		public string Val1 { get { return GetValue(); } set { SetValue(value); } }

		[SmartCookieSettings("v32")]
		public int Val2 { get { return GetValueInt(); } set { SetValue(value); } }
	}

	public class TestCookie4 : SmartCookie
	{
		private static byte[] _EncryptionKey;
		
		public TestCookie4(HttpContextBase context)
			: base(context)
		{
		}

		static public void SetEncryptionKey(byte []encrytpionKey)
		{
			_EncryptionKey = encrytpionKey;
		}

		[SmartCookieSkip]
		public int UserId
		{
			get
			{
				if (string.IsNullOrEmpty(IV) || string.IsNullOrEmpty(UserIdEncrypted))
					return 0;

				var ivBytes = Convert.FromBase64String(IV);
				var encryptedBytes = Convert.FromBase64String(UserIdEncrypted);

				using (var aes = new AesManaged())
				using(var decryptor = aes.CreateDecryptor(_EncryptionKey, ivBytes))
				using(var ms = new MemoryStream(encryptedBytes))
				using(var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
				{
					var decrypted = new byte[4];
					cs.Read(decrypted, 0, 4);
					return BitConverter.ToInt32(decrypted, 0);
				}
			}

			set
			{
				if (value <= 0)
				{
					IV = null;
					UserIdEncrypted = null;
					return;
				}

				using (var aes = new AesManaged())
				{
					aes.GenerateIV();
					IV = Convert.ToBase64String(aes.IV);

					using(var encryptor = aes.CreateEncryptor(_EncryptionKey, aes.IV))
					using(var ms = new MemoryStream())
					using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
					{
						var bytes = BitConverter.GetBytes(value);
						cs.Write(bytes, 0, bytes.Length);
						cs.FlushFinalBlock();

						byte[] encryptedBytes = ms.ToArray();
						UserIdEncrypted = Convert.ToBase64String(encryptedBytes);
					}
				}
			}
		}

		[SmartCookieSettings("i")]
		protected string IV { get { return GetValue(); } set { SetValue(value); } }
		
		[SmartCookieSettings("u")]
		protected string UserIdEncrypted { get { return GetValue(); } set { SetValue(value); } }
	}

}
