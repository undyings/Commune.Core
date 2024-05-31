using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class ParentBox : ObjectBox
	{
		static LinkRow[] LoadChildRows(BoxDbContext context, int[] objectIds)
		{
			return context.Links.Where(link => objectIds.Contains(link.ParentId))
				.OrderBy(link => link.ParentId).ThenBy(link => link.TypeId).ThenBy(link => link.LinkIndex)
				.ToArray();
		}

		public readonly SingleIndexLink<LinkRow> ChildByLinkId;
		public readonly OrderedIndexLink<LinkRow> ChildsByObjectIdWithKind;
		public readonly MultiIndexLink<LinkRow> ChildsByObjectId;
		public readonly OrderedIndexLink<LinkRow> ChildsByChildIdWithKind;

		public ParentBox(DataLayer dataLayer, BoxDbContext context, Func<BoxDbContext, IQueryable<ObjectRow>> rowsLoader) :
			base(dataLayer, context, rowsLoader)
		{
			LinkRow[] childRows = ParentBox.LoadChildRows(context, base.AllObjectIds);

			TableLink<LinkRow> childTable = new(childRows, LinkType.LinkById, LinkType.LinksByParentIdAndTypeId,
					LinkType.LinksByParentId, LinkType.LinksByChildId, LinkType.LinksByChildIdAndTypeId
			);

			this.ChildByLinkId = new SingleIndexLink<LinkRow>(childTable, LinkType.LinkById);
			this.ChildsByObjectIdWithKind = new OrderedIndexLink<LinkRow>(childTable, LinkType.LinksByParentIdAndTypeId);
			this.ChildsByObjectId = new MultiIndexLink<LinkRow>(childTable, LinkType.LinksByParentId);
			this.ChildsByChildIdWithKind = new OrderedIndexLink<LinkRow>(childTable, LinkType.LinksByChildIdAndTypeId);
		}

		public ParentBox(DataLayer dataLayer, BoxDbContext context) :
			this(dataLayer, context, db => Array.Empty<ObjectRow>().AsQueryable())
		{
		}


		public TableLink<LinkRow> ChildTable
		{
			get { return ChildByLinkId.TableLink; }
		}

		public LinkRow CreateLinkRow(int parentId, int typeId, int linkIndex, int childId)
		{
			LinkRow link = new()
			{
				LinkId = dataLayer.GeneratePrimaryKey(BoxTableNames.LinkTable),
				ParentId = parentId,
				TypeId = typeId,
				LinkIndex = linkIndex,
				ChildId = childId
			};

			return link;
		}


		public override void Update(BoxDbContext context, bool syncChangesWithDb)
		{
			base.Update(context, false);

			LinkRow[] dbLinkRows = LoadChildRows(context, base.AllObjectIds);
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
