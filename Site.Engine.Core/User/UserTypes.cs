using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;

namespace Site.Engine
{
  public class UserType
  {
    public const int User = 1000;

		public readonly static JsonUniqueBlank<LoginId> JsonId = LoginId.Blank;

		public readonly static PropertyBlank<string> Email = new(1100, DataBox.StringValue);

		public readonly static PropertyBlank<string> Family = new(1101, DataBox.StringValue);
    public readonly static PropertyBlank<string> FirstName = new(1102, DataBox.StringValue);

    public readonly static PropertyBlank<string> PasswordHash = new(1103, DataBox.StringValue);
		public readonly static PropertyBlank<DateTime?> PasswordRestoreTime = new(1104, DataBox.DateTimeNullableValue);

		public readonly static PropertyBlank<bool> NotConfirmed = new(1111, DataBox.BoolValue);
		public readonly static PropertyBlank<DateTime?> BannedUntil = new(1112, DataBox.DateTimeNullableValue);
		public readonly static PropertyBlank<bool> LockedUserIds = new(1113, DataBox.BoolValue);
		public readonly static PropertyBlank<bool> IsModerator = new(1114, DataBox.BoolValue);

		public readonly static PropertyBlank<string> Interests = new(1121, DataBox.StringValue);
		public readonly static PropertyBlank<string> Country = new(1122, DataBox.StringValue);
		public readonly static PropertyBlank<string> City = new(1123, DataBox.StringValue);
		public readonly static PropertyBlank<string> AboutMe = new(1124, DataBox.StringValue);


		//public readonly static RowPropertyBlank<bool> IsFemale = DataBox.Create(1110, DataBox.BoolValue);
	}


	public class ReviewType
  {
    public const int Review = 2000;

    public readonly static JsonUniqueBlank<NameId> ClientName = NameId.Blank;

    public readonly static PropertyBlank<int> Evaluation = new(2101, DataBox.IntValue);
    public readonly static PropertyBlank<string> Text = new(2111, DataBox.StringValue);
    public readonly static PropertyBlank<string> Answer = new(2121, DataBox.StringValue);
  }
}
