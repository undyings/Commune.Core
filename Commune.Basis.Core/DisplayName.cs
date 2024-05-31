using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Basis
{
	public class DisplayName
	{
		public readonly string Name;
		public readonly string Display;

		public DisplayName(string name, string display)
		{
			this.Name = name;
			this.Display = display;
		}
	}
}
