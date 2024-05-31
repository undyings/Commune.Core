using Commune.Basis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Site.Engine
{
	public class EditorSelector
	{
		readonly Dictionary<string, BaseTunes> tunesByDesignKind = new Dictionary<string, BaseTunes>();

		public EditorSelector()
		{
		}

		public EditorSelector(params SectionTunes[] tunes)
		{
			foreach (SectionTunes tune in tunes)
			{
				tunesByDesignKind[tune.DesignKind] = tune;
			}
		}

		public EditorSelector(params UnitTunes[] tunes)
		{
			foreach (UnitTunes tune in tunes)
			{
				tunesByDesignKind[tune.DesignKind] = tune;
			}
		}

		public EditorSelector(params GroupTunes[] tunes)
		{
			foreach (GroupTunes tune in tunes)
			{
				tunesByDesignKind[tune.DesignKind] = tune;
			}
		}

		public string[] AllKinds
		{
			get { return tunesByDesignKind.Keys.ToArray(); }
		}

		public string GetDisplayName(string designKind)
		{
			return FindTunes(designKind)?.DisplayName ?? "";
		}

		public BaseTunes? FindTunes(string designKind)
		{
			return tunesByDesignKind.Find(designKind);
		}
	}
}
