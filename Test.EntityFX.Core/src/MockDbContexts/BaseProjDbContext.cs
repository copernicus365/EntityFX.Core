using System;
using System.Collections.Generic;

using DotNetXtensions;

using EntityFX.Core;

using Microsoft.EntityFrameworkCore;

namespace Test.EntityFX.Core.Mock
{
	public class BaseProjDbContext : DbContext
	{
		public static bool EnableLoggingDefault = false;

		public bool EnableLogging = EnableLoggingDefault;

		public bool InMemory => InMemoryDatabaseName != null;

		public string InMemoryDatabaseName { get; set; }

		readonly string _connStr;

		public BaseProjDbContext(string connStr)
			: base()
		{
			this._connStr = connStr;
		}

		public void ValidateInMemoryDatabase()
		{
			if(InMemoryDatabaseName != null) {

				if(InMemoryDatabaseName.Length < 1 || InMemoryDatabaseName.IsTrimmable())
					throw new ArgumentNullException(nameof(InMemoryDatabaseName));
			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if(InMemory) {
				ValidateInMemoryDatabase();

				var options = //new DbContextOptionsBuilder<BaseProjDbContext>()
					optionsBuilder
					  .UseInMemoryDatabase(databaseName: InMemoryDatabaseName)
					  .Options;
			}
			else {
				optionsBuilder.UseSqlServer(_connStr);
			}

			if(EnableLogging) {

				optionsBuilder.SetLog((state, val) => {
					$" - sqllog {(InMemory ? InMemoryDatabaseName : _connStr)}: {state}]  {val}".Print();
				});
			}

			base.OnConfiguring(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.SetPrecisionDecimals(new KeyValuePair<int, int>(36, 20));
			base.OnModelCreating(modelBuilder);
		}

	}
}
