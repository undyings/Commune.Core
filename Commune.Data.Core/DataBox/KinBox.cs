using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class KinBox : ParentBox
	{
		static LinkRow[] LoadParentRows(BoxDbContext context, int[] objectIds)
		{
			return context.Links.Where(link => objectIds.Contains(link.ChildId))
				.OrderBy(link => link.ParentId).ThenBy(link => link.TypeId).ThenBy(link => link.LinkIndex)
				.ToArray();
		}

		public readonly SingleIndexLink<LinkRow> ParentByLinkId;
		public readonly MultiIndexLink<LinkRow> ParentsByObjectId;
		public readonly OrderedIndexLink<LinkRow> ParentsByObjectIdWithKind;
		public readonly OrderedIndexLink<LinkRow> ObjectsByParentIdWithKind;

		public KinBox(DataLayer dataLayer, BoxDbContext context, Func<BoxDbContext, IQueryable<ObjectRow>> rowsLoader) :
			base(dataLayer, context, rowsLoader)
		{
			LinkRow[] parentRows = LoadParentRows(context, base.AllObjectIds);

			TableLink<LinkRow> parentTable = new(parentRows, LinkType.LinkById, LinkType.LinksByChildId, LinkType.LinksByChildIdAndTypeId, LinkType.LinksByParentIdAndTypeId);

			this.ParentByLinkId = new SingleIndexLink<LinkRow>(parentTable, LinkType.LinkById);
			this.ParentsByObjectId = new MultiIndexLink<LinkRow>(parentTable, LinkType.LinksByChildId);
			this.ParentsByObjectIdWithKind = new OrderedIndexLink<LinkRow>(parentTable, LinkType.LinksByChildIdAndTypeId);
			this.ObjectsByParentIdWithKind = new OrderedIndexLink<LinkRow>(parentTable, LinkType.LinksByParentIdAndTypeId);
		}


		public TableLink<LinkRow> ParentTable
		{
			get { return ParentByLinkId.TableLink; }
		}

		public override void Update(BoxDbContext context, bool syncChangesWithDb)
		{
			base.Update(context, false);

			LinkRow[] dbLinkRows = LoadParentRows(context, base.AllObjectIds);
			ChildByLinkId.TableLink.SyncChanges(context.Links, dbLinkRows, LinkType.SyncChanges);

			if (syncChangesWithDb)
				context.SaveChanges();
		}

		public override long DataChangeTick
		{
			get
			{
				return base.DataChangeTick + ChildTable.DataChangeTick + ChildTable.RowListChangeTick;
			}
		}


	}
}
