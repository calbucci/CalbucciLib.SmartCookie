using System;
using System.Runtime.CompilerServices;

namespace CalbucciLib
{
	public class SmartCookieMultiValue
	{
		public SmartCookie SmartCookie { get; internal set; }
		public string PropertyName { get; internal set; }


		public void SetValue(string value, [CallerMemberName] string propertyName = "")
		{
			SmartCookie.Internal_SetValues(GetType(), PropertyName, propertyName, value);
		}

		public void SetValue(int value, [CallerMemberName] string propertyName = "")
		{
			SmartCookie.Internal_SetValues(GetType(), PropertyName, propertyName, value.ToString());
		}

		public void SetValue(long value, [CallerMemberName] string propertyName = "")
		{
			SmartCookie.Internal_SetValues(GetType(), PropertyName, propertyName, value.ToString());
		}

		public void SetValue(bool value, [CallerMemberName] string propertyName = "")
		{
			SmartCookie.Internal_SetValues(GetType(), PropertyName, propertyName, SmartCookie.ConvertToString(value));
		}

		public void SetValue(double value, [CallerMemberName] string propertyName = "")
		{
			SmartCookie.Internal_SetValues(GetType(), PropertyName, propertyName, SmartCookie.ConvertToString(value));
		}

		public void SetValue(DateTime? value, [CallerMemberName] string propertyName = "")
		{
			SmartCookie.Internal_SetValues(GetType(), PropertyName, propertyName, SmartCookie.ConvertToString(value));
		}


		public string GetValue([CallerMemberName] string propertyName = "")
		{
			return SmartCookie.Internal_GetValues(GetType(), PropertyName, propertyName);
		}

		public int GetValueInt([CallerMemberName] string propertyName = "")
		{
			return SmartCookie.ConvertToInt(SmartCookie.Internal_GetValues(GetType(), PropertyName, propertyName));
		}

		public long GetValueLong([CallerMemberName] string propertyName = "")
		{
			return SmartCookie.ConvertToLong(SmartCookie.Internal_GetValues(GetType(), PropertyName, propertyName));
		}

		public double GetValueDouble([CallerMemberName] string propertyName = "")
		{
			return SmartCookie.ConvertToDouble(SmartCookie.Internal_GetValues(GetType(), PropertyName, propertyName));
		}

		public DateTime? GetValueDateTime([CallerMemberName] string propertyName = "")
		{
			return SmartCookie.ConvertToDateTime(SmartCookie.Internal_GetValues(GetType(), PropertyName, propertyName));
		}

		public bool GetValueBool([CallerMemberName] string propertyName = "")
		{
			return SmartCookie.ConvertToBool(SmartCookie.Internal_GetValues(GetType(), PropertyName, propertyName));
		}
	}
}
