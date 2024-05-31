using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Data
{
	public class BoxDbContext : DbContext
	{
		public BoxDbContext(DbContextOptions options) :
			base(options)
		{
		}

		public DbSet<ObjectRow> Objects { get; set; }
		public DbSet<PropertyRow> Properties { get; set; }
		public DbSet<LinkRow> Links { get; set; }

		//public DbSet<PrimaryKeyRow> PrimaryKeys { get; set; }
	}
}
