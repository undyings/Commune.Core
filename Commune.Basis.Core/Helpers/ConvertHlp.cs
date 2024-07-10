using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Basis
{
	public class ConvertHlp
	{
		public static int? ToInt(object value)
		{
			try
			{
				if (value == null)
					return null;
				return Convert.ToInt32(value);
			}
			catch
			{
			}
			return null;
		}

		public static int? ToInt(string? value)
		{
			int result;
			if (int.TryParse(value, out result))
				return result;
			return null;
		}

		public static string? FromInt(int? value)
		{
			if (value == null)
				return null;
			return value.Value.ToString(CultureInfo.InvariantCulture);
		}

		public static long? ToLong(string? value)
		{
			long result;
			if (long.TryParse(value, out result))
				return result;
			return null;
		}

		public static string? FromLong(long? value)
		{
			if (value == null)
				return null;
			return value.Value.ToString(CultureInfo.InvariantCulture);
		}

		public static float? ToFloat(object value)
		{
			try
			{
				if (value == null)
					return null;

				if (value is string)
				{
					string s = ((string)value).Replace(',', '.');
					float result;
					if (float.TryParse(s, CultureInfo.InvariantCulture, out result))
						return result;
					return null;
				}

				return Convert.ToSingle(value);
			}
			catch
			{
				return null;
			}
		}

		public static double? ToDouble(object value)
		{
			try
			{
				if (value == null)
					return null;

				if (value is string)
				{
					string s = ((string)value).Replace(',', '.');
					double result;
					if (double.TryParse(s, CultureInfo.InvariantCulture, out result))
						return result;
					return null;
				}

				return Convert.ToDouble(value);
			}
			catch
			{
				return null;
			}
		}

		public static decimal? ToDecimal(object value)
		{
			try
			{
				if (value == null)
				{
					return null;
				}

				if (value is string)
				{
					return decimal.Parse(((string)value).Replace(',', '.'), CultureInfo.InvariantCulture);
				}

				return Convert.ToDecimal(value);
			}
			catch
			{
			}

			return null;
		}

		public static double? ToDouble(string? value)
		{
			if (value == null)
				return null;

			if (double.TryParse(value.Replace(',', '.'), CultureInfo.InvariantCulture, out double result))
				return result;
			return null;
		}

		public static string? FromDouble(double? value)
		{
			if (value == null)
				return null;
			return value.Value.ToString(CultureInfo.InvariantCulture);
		}

		public static float? ToFloat(string? value)
		{
			if (value == null)
				return null;

			if (float.TryParse(value.Replace(',', '.'), CultureInfo.InvariantCulture, out float result))
				return result;
			return null;
		}

		public static string? FromFloat(float? value)
		{
			if (value == null)
				return null;
			return value.Value.ToString(CultureInfo.InvariantCulture);
		}

		public static bool? ToBool(string? value)
		{
			if (value == "1")
				return true;
			if (value == "0")
				return false;
			return null;
		}

		public static string? FromBool(bool? value)
		{
			if (value == true)
				return "1";
			if (value == false)
				return "0";
			return null;
		}

		public static DateTime? TicksToDateTime(string? value)
		{
			long? ticks = ToLong(value);
			if (ticks == null)
				return null;
			return new DateTime(ticks.Value);
		}

		public static string? TicksFromDateTime(DateTime? value)
		{
			if (value == null)
				return null;
			return value.Value.Ticks.ToString(CultureInfo.InvariantCulture);
		}

		//public static DateTime? ToDateTime(string? value)
		//{
		//	if (value == null && value == "")
		//		return null;

		//	DateTime date;
		//	if (DateTime.TryParse(value, out date))
		//		return date;

		//	return null;
		//}

	}
}
