using Commune.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class NameId
	{
		public readonly static JsonUniqueBlank<NameId> Blank = new();
		public static string GetName(LightHead obj)
		{
			return Blank.Get(obj.Head)?.Name ?? "";
		}

		public string Name { get; set; }
		public NameId()
		{
			this.Name = "";
		}

		public NameId(string name)
		{
			this.Name = name;
		}
	}

	public class KindId
	{
		public readonly static JsonUniqueBlank<KindId> Blank = new();

		public string Kind { get; set; }
		public KindId()
		{
			this.Kind = "";
		}

		public KindId(string kind)
		{
			this.Kind = kind;
		}
	}

	public class ParentNameId
	{
		public readonly static JsonUniqueBlank<ParentNameId> Blank = new();

		public int Parent { get; set; }
		public string Name { get; set; }

		public ParentNameId()
		{
			this.Name = "";
		}

		public ParentNameId(int parent, string name)
		{
			this.Parent = parent;
			this.Name = name;
		}
	}

	public class LoginId
	{
		public readonly static JsonUniqueBlank<LoginId> Blank = new();

		public string Auth { get; set; }
		public string Login { get; set; }

		public LoginId()
		{
			this.Auth = "";
			this.Login = "";
		}

		public LoginId(string auth, string login)
		{
			this.Auth = auth;
			this.Login = login;
		}
	}
}
