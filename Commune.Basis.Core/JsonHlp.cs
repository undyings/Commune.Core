﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Commune.Basis
{
	public class JsonHlp
	{
		public readonly static JsonSerializerOptions Cyrillic = new() 
		{ 
			IncludeFields = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic) 
		};

		public static T SafeDeserialize<T>(string json) where T : new()
		{
			if (json == "")
				return new T();

			try
			{
				return JsonSerializer.Deserialize<T>(json, Cyrillic) ?? new T();
			}
			catch
			{
				return new T();
			}
		}

		public static string Serialize<T>(T obj)
		{
			return JsonSerializer.Serialize(obj, typeof(T), Cyrillic);
		}

		public static T SafeLoad<T>(string path) where T : new()
		{
			try
			{
				if (!File.Exists(path))
					return new T();

				return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Cyrillic) ?? new T();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "");
				return new T();
			}
		}

		public static void Save<T>(T obj, string path)
		{
			string json = JsonSerializer.Serialize(obj, Cyrillic);
			File.WriteAllText(path, json);
		}
	}
}
