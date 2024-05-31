using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;

namespace Site.Engine
{
  public class LightSection : LightKin
  {
    readonly object lockObj = new object();

    readonly RawCache<LightSection[]> subpagesCache;

    readonly RawCache<Dictionary<int, LightUnit>> unitForPaneCache;

    readonly SectionStorage store;
    public LightSection(SectionStorage store, int pageId) :
      base(store.sectionBox, pageId)
    {
      this.store = store;

      this.subpagesCache = new Cache<long, LightSection[]>(
        delegate
        {
          int[] subpageIds = AllChildIds(SectionType.SubsectionLinks);
          List<LightSection> subpages = new List<LightSection>(subpageIds.Length);
          foreach (int subpageId in subpageIds)
          {
            LightSection? subpage = store.FindSection(subpageId);
            if (subpage != null)
              subpages.Add(subpage);
          }
          return SorterHlp.Sort(subpages, this.Get(SectionType.SortKind));
        },
        delegate { return 0; }
      );

      this.unitForPaneCache = new Cache<long, Dictionary<int, LightUnit>>(
        delegate
        {
          int[] unitIds = AllChildIds(SectionType.UnitForPaneLinks);
          Dictionary<int, LightUnit> unitForPane = new(unitIds.Length);
          foreach (LinkRow link in AllChildRows(SectionType.UnitForPaneLinks))
          {
            int unitId = link.ChildId;
            int paneIndex = link.LinkIndex;
            LightUnit? unit = store.FindUnit(unitId);
            if (unit != null)
              unitForPane[paneIndex] = unit;
          }
          return unitForPane;
        },
        delegate { return 0; }
      );
    }

    public bool IsMenu
    {
      get { return GetParentId(SectionType.SubsectionLinks) == null; }
    }

    public LightSection? ParentSection
    {
      get
      {
        int? parentId = GetParentId(SectionType.SubsectionLinks);
        return store.FindSection(parentId);
      }
    }

    public LightSection[] Subsections
    {
      get
      {
        lock (lockObj)
          return subpagesCache.Result;
      }
    }

    public LightUnit? UnitForPane(int paneIndex)
    {
      lock (lockObj)
        return unitForPaneCache.Result.Find(paneIndex);
    }

    public LightUnit[] AllUnits
    {
      get
      {
        lock (lockObj)
          return unitForPaneCache.Result.Values.ToArray();
      }
    }

    public string NameInMenu
    {
      get
      {
        string menuName = this.Get(SectionType.NameInMenu);
        if (!StringHlp.IsEmpty(menuName))
          return menuName;
        return this.Get(SectionType.Title).Name;
      }
    }
  }
}
