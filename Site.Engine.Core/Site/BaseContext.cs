using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Commune.Basis;
using Commune.Data;
using Commune.Task;

namespace Site.Engine
{
	public abstract class BaseContext : IContext
	{
		public volatile static IContext Default;

		protected readonly static object lockObj = new();

		//readonly SiteSettings siteSettings;
		//public SiteSettings SiteSettings
		//{
		//	get { return siteSettings; }
		//}

		public abstract SiteSettings SiteSettings { get; }

		readonly TaskPull pull;
		public TaskPull Pull
		{
			get { return pull; }
		}

		readonly string contentRootPath;
		public string ContentRootPath
		{
			get { return contentRootPath; }
		}
		readonly string webRootPath;
		public string WebRootPath
		{
			get { return webRootPath; }
		}
		readonly string imagesPath;
		public string ImagesPath
		{
			get { return imagesPath; }
		}
		readonly DataLayer userConnection;
		public DataLayer UserConnection
		{
			get { return userConnection; }
		}
		readonly DataLayer fabricConnection;
		public DataLayer FabricConnection
		{
			get { return fabricConnection; }
		}

		//readonly IDataLayer orderConnection;
		//public IDataLayer OrderConnection
		//{
		//	get { return orderConnection; }
		//}

		readonly EditorSelector sectionEditorSelector;
		public EditorSelector SectionEditorSelector
		{
			get { return sectionEditorSelector; }
		}

		readonly EditorSelector unitEditorSelector;
		public EditorSelector UnitEditorSelector
		{
			get { return unitEditorSelector; }
		}

		//readonly ContextTunes contextTunes;
		//public ContextTunes ContextTunes
		//{
		//	get { return contextTunes; }
		//}

		public BaseContext(string contentRootPath, string webRootPath,
			EditorSelector sectionEditorSelector, EditorSelector unitEditorSelector,
			DataLayer userConnection, DataLayer fabricConnection)
		{
			this.contentRootPath = contentRootPath;
			this.webRootPath = webRootPath;
			this.imagesPath = Path.Combine(webRootPath, "images");
			this.userConnection = userConnection;
			this.fabricConnection = fabricConnection;
			this.sectionEditorSelector = sectionEditorSelector;
			this.unitEditorSelector = unitEditorSelector;

			this.pull = new TaskPull(
				new ThreadLabel[] { Labels.Service },
				TimeSpan.FromMinutes(15)
			);

			//Pull.StartTask(Labels.Service, SiteTasks.SitemapXmlChecker(this, webRootPath));

			//if (contextTunes.GetTune("reviews"))
			//{
			//	this.reviewsCache = new Cache<long, LightObject[]>(
			//		delegate
			//		{
			//			using BoxDbContext dbContext = userConnection.Create();

			//			ObjectBox reviewBox = new(userConnection, dbContext, db => db.Objects.ForType(ReviewType.Review).OrderByDescending(obj => obj.ActFrom));

			//			return ArrayHlp.Convert(reviewBox.AllObjectIds, delegate (int reviewId)
			//				{ return new LightObject(reviewBox, reviewId); }
			//			);
			//		},
			//		delegate { return userDataChangeTick; }
			//	);
			//}
		}

		public abstract IStore Store { get; }

		protected long dataChangeTick = 0;
		public void UpdateStore()
		{
			lock (lockObj)
				dataChangeTick++;
		}

		//readonly RawCache<LightObject[]>? reviewsCache = null;
		//public LightObject[] Reviews
		//{
		//	get
		//	{
		//		if (reviewsCache == null)
		//			return new LightObject[0];

		//		lock (lockObj)
		//			return reviewsCache.Result;
		//	}
		//}

		//protected long userDataChangeTick = 0;
		//public void UpdateUserData()
		//{
		//	lock (lockObj)
		//		userDataChangeTick++;
		//}
	}
}
