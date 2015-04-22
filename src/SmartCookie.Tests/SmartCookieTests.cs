using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CalbucciLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace CalbucciLib.Tests
{
	[TestClass()]
	public class SmartCookieTests
	{

		static public T InitCookie<T>() where T: SmartCookie
		{
			HttpContext rawContext = new HttpContext(
				new HttpRequest(null, "http://blog.calbucci.com", null),
				new HttpResponse(null));
			HttpContextBase context = new HttpContextWrapper(rawContext);

			var cookie = Activator.CreateInstance(typeof (T), new object[] {context}) as T;
			return cookie;
		}
		
		

		[TestMethod]
		public void SetAndGetValues()
		{
			var cookie = InitCookie<TestCookie1>();

			cookie.Val1 = "abc";
			cookie.Val2 = 123;
			cookie.Val3 = 456;
			cookie.Val4 = 7.89;
			cookie.Val5 = new DateTime(2015, 4, 21);
			cookie.Val6 = true;

			Assert.AreEqual(cookie.Val1, "abc");
			Assert.AreEqual(cookie.Val2, 123);
			Assert.AreEqual(cookie.Val3, (long)456);
			Assert.AreEqual(cookie.Val4, 7.89);
			var dt = cookie.Val5;
			Assert.IsTrue(dt != null && dt.Value.Year == 2015 && dt.Value.Month == 4 && dt.Value.Day == 21);
			Assert.AreEqual(cookie.Val6, true);

		}

		[TestMethod]
		public void CheckResponseRawCookie()
		{
			var cookie = InitCookie<TestCookie1>();

			cookie.Val1 = "abc";
			cookie.Val2 = 123;
			cookie.Val3 = 456;
			cookie.Val4 = 7.89;
			cookie.Val5 = new DateTime(2015, 4, 21);
			cookie.Val6 = true;

			var rawCookies = cookie.Context.Response.Cookies;

			Assert.AreEqual(rawCookies["Val1"].Value, "abc");
			Assert.AreEqual(rawCookies["Val2"].Value, "123");
			Assert.AreEqual(rawCookies["Val3"].Value, "456");
			Assert.AreEqual(rawCookies["Val4"].Value, "7.89");
			Assert.AreEqual(rawCookies["Val5"].Value, (new DateTime(2015, 4, 21).ToString("s")));
			Assert.AreEqual(rawCookies["Val6"].Value, "1");

		}

		[TestMethod]
		public void TestMultiValue()
		{
			var cookie = InitCookie<TestCookie2>();

			cookie.Values1.Val1 = "abc";
			cookie.Values1.Val2 = 123;

			cookie.Values2.Val1 = "def";
			cookie.Values2.Val2 = 456;

			Assert.AreEqual(cookie.Values1.Val1, "abc");
			Assert.AreEqual(cookie.Values1.Val2, 123);

			Assert.AreEqual(cookie.Values2.Val1, "def");
			Assert.AreEqual(cookie.Values2.Val2, 456);
		}

		[TestMethod]
		public void CheckAttributes()
		{
			var cookie = InitCookie<TestCookie3>();

			cookie.Val1 = "abcdefghijklmnopqrstuvwxyz";

			cookie.MVal2.Val1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			cookie.MVal2.Val2 = 123;

			var rawCookies = cookie.Context.Response.Cookies;

			HttpCookie rawVal1 = rawCookies["v1"];
			Assert.IsNotNull(rawVal1);
			Assert.AreEqual(rawVal1.Domain, "domain.com");
			double totalDaysDelta = Math.Abs((DateTime.UtcNow.AddDays(30) - rawVal1.Expires).TotalDays);
			Assert.IsTrue(totalDaysDelta < 1);
			Assert.AreEqual(rawVal1.HttpOnly, true);
			Assert.AreEqual(rawVal1.Secure, true);
			Assert.AreEqual(rawVal1.Path, "/a/");
			Assert.AreEqual(rawVal1.Value.Length, 10); //MaxLength
			Assert.AreEqual(rawVal1.Value, "abcdefghij");


			HttpCookie rawVal2 = rawCookies["v2"];
			Assert.IsNotNull(rawVal2);
			Assert.AreEqual(rawVal2["v31"].Length, 8);
			Assert.AreEqual(rawVal2["v31"], "ABCDEFGH");
			Assert.AreEqual(rawVal2["v32"], "123");
		}

		[TestMethod]
		public void TestEncryption()
		{
			using (var aes = new AesManaged())
			{
				// Just for testing since we don't need to persist the aes.Key
				aes.GenerateKey();
				TestCookie4.SetEncryptionKey(aes.Key);
			}

			

			var cookie = InitCookie<TestCookie4>();

			cookie.UserId = 123;

			Assert.AreEqual(cookie.UserId, 123);

			var rawCookies = cookie.Context.Response.Cookies;
			Assert.IsNotNull(rawCookies["i"]);
			Assert.IsNotNull(rawCookies["u"]);

			cookie.UserId = 0;
			Assert.IsTrue(rawCookies["i"].Expires < DateTime.UtcNow);
			Assert.IsTrue(rawCookies["u"].Expires < DateTime.UtcNow);
			
		}




	}
}
