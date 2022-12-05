using Microsoft.EntityFrameworkCore;
using RFIDUnlocker.GUI.Models;

namespace RFIDUnlocker.GUI.Services.Data
{
	internal class ApplicationContext : DbContext
	{
		public DbSet<Card> Cards { get; set; }
		public DbSet<ActionInfo> Actions { get; set; }

		public ApplicationContext()
		{
			Database.Migrate();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder builder)
		{
			builder.UseSqlite("Filename=db.sqlite");
		}
	}
}
