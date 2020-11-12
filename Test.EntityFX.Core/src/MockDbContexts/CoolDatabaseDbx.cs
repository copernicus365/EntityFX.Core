using Microsoft.EntityFrameworkCore;

namespace Test.EntityFX.Core.Mock
{
	public class CoolDatabaseDbx : BaseProjDbContext
	{
		public CoolDatabaseDbx() : base("connStr....")
		{
			//EnableLogging = true;
		}

		public DbSet<WidgetEnt> Widgets { get; set; }

	}
}
