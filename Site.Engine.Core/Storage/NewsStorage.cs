using System;
using System.Collections.Generic;
using System.Text;
using Commune.Basis;
using Commune.Data;

namespace Site.Engine
{
  public class NewsStorage
  {
    public static NewsStorage Load(DataLayer fabricConnection, BoxDbContext dbContext, int actualNewsCount)
    {
      ObjectBox actualNewsBox = new(fabricConnection, dbContext, db => db.Objects.ForType(NewsType.News).OrderByDescending(obj => obj.ActFrom).Take(actualNewsCount));

      ObjectHeadBox headNewsBox = new(fabricConnection, dbContext, db => db.Objects.ForType(NewsType.News));

      return new NewsStorage(actualNewsBox, headNewsBox);
    }

    public readonly ObjectBox actualNewsBox;
    public readonly ObjectHeadBox headNewsBox;

    public NewsStorage(ObjectBox actualNewsBox, ObjectHeadBox headNewsBox)
    {
      this.actualNewsBox = actualNewsBox;
      this.headNewsBox = headNewsBox;

      this.Actual = ArrayHlp.Convert(actualNewsBox.AllObjectIds, delegate (int newsId)
      {
        return new LightObject(actualNewsBox, newsId);
      });
    }

    public readonly LightObject[] Actual;

    public void FillLinks(TranslitLinks links)
    {
      foreach (ObjectRow newsRow in headNewsBox.ObjectById.TableLink.AllRows)
      {
        links.AddLink(Site.Novosti, newsRow.ObjectId, NewsType.Title.Get(newsRow).Name, newsRow.ActTo);
      }
    }
  }
}
