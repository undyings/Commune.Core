using System;
using System.Collections.Generic;
using Commune.Basis;
using Commune.Data;

namespace Site.Engine
{
  public class RedirectStorage
  {
    public static RedirectStorage Load(DataLayer fabricConnection, BoxDbContext dbContext)
    {
      ObjectBox redirectBox = new ObjectBox(fabricConnection, dbContext, db => db.Objects.ForType(RedirectType.Redirect).OrderBy(obj => obj.JsonId));

      return new RedirectStorage(redirectBox);
    }

    readonly Dictionary<string, LightObject> redirectByUrl = new Dictionary<string, LightObject>();
    readonly Dictionary<int, LightObject> redirectById = new Dictionary<int, LightObject>();
    public readonly LightObject[] All;

    public LightObject? Find(string url)
    {
      if (StringHlp.IsEmpty(url))
        return null;

      return redirectByUrl.Find(url.ToLower());
    }

    public LightObject? Find(int? deadLinkId)
    {
      if (deadLinkId == null)
        return null;

      return redirectById.Find(deadLinkId.Value);
    }

    public readonly ObjectBox redirectBox;

    public RedirectStorage(ObjectBox redirectBox)
    {
      this.redirectBox = redirectBox;

      foreach (int redirectId in redirectBox.AllObjectIds)
      {
        LightObject redirect = new LightObject(redirectBox, redirectId);

        redirectById[redirectId] = redirect;

        string deadUrl = redirect.Get(RedirectType.From)?.Name ?? "";
        if (StringHlp.IsEmpty(deadUrl))
          continue;
        redirectByUrl[deadUrl.ToLower()] = redirect;
      }

      this.All = redirectByUrl.Values.ToArray();
    }
  }
}
