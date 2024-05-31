using Commune.Basis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class UniqueChecker
	{
		readonly ObjectHeadBox objectBox;
		readonly int[] allLoadObjectIds;
		public UniqueChecker(ObjectHeadBox objectBox)
		{
			this.objectBox = objectBox;
			this.allLoadObjectIds = objectBox.AllObjectIds;
		}

		public bool IsUniqueRow(BoxDbContext context, ObjectRow row)
		{
			return IsUniqueKey(context, row.ObjectId, row.TypeId, row.JsonId, row.ActFrom);
		}

		public bool IsUniqueKey(BoxDbContext context, long? objectId, int typeId, string jsonIds, DateTime? actFrom)
		{
			bool isTypeWithUniqueActFrom = actFrom != null;
			foreach (ObjectRow row in objectBox.ObjectsByJsonId.Rows(jsonIds))
			{
				if (row.ObjectId == objectId)
					continue;
				if (row.TypeId != typeId)
					continue;
				if (isTypeWithUniqueActFrom && row.ActFrom != actFrom)
					continue;
				return false;
			}

			int count;
			if (isTypeWithUniqueActFrom)
				count = context.Objects.Count(obj => obj.TypeId == typeId && obj.JsonId == jsonIds && obj.ActFrom == actFrom && !allLoadObjectIds.Contains(obj.ObjectId));
			else
				count = context.Objects.Count(obj => obj.TypeId == typeId && obj.JsonId == jsonIds && !allLoadObjectIds.Contains(obj.ObjectId));

			return count == 0;
		}
	}
}