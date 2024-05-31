using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Basis
{
	public class TuneContainer<T>
	{
		readonly Dictionary<string, T> tuneByName = new();
		readonly Dictionary<string, string> settingByName = new();

		public TuneContainer()
		{
		}

		public T? GetTune(string tuneName)
		{
			T? tune;
			if (tuneByName.TryGetValue(tuneName, out tune))
				return tune;
			return default;
		}

		public void WithTune(string tuneName, T value)
		{
			if (tuneName == null)
				return;
			tuneByName[tuneName] = value;
		}

		public string GetSetting(string settingName)
		{
			string? setting;
			if (settingByName.TryGetValue(settingName, out setting))
				return setting;
			return "";
		}

		public void WithSetting(string settingName, string value)
		{
			if (settingName == null)
				return;
			settingByName[settingName] = value;
		}
	}
}
