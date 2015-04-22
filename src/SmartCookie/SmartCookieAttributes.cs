using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalbucciLib
{
	/// <summary>
	/// Indicates a property that should not be saved to a cookie
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class SmartCookieSkipAttribute : Attribute
	{
	}


	/// <summary>
	/// Define how this Property will be converted to a cookie
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class SmartCookieSettingsAttribute : Attribute
	{
		public SmartCookieSettingsAttribute()
		{ }

		public SmartCookieSettingsAttribute(string name)
		{
			Name = name;
		}

		public SmartCookieSettingsAttribute(string name, int expiresInDays)
		{
			Name = name;
			ExpiresInDays = expiresInDays;
		}

		public string Name { get; set; }
		public int ExpiresInDays { get; set; }
		public bool HttpOnly { get; set; }
		public string Domain { get; set; }
		public string Path { get; set; }
		public bool Secure { get; set; }
		public int MaxLength { get; set; }

	}
}
