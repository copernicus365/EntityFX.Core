using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityFX.Core
{
	public static class DbContextCommon
	{
		/// <summary>
		/// https://stackoverflow.com/questions/43277154/entity-framework-core-setting-the-decimal-precision-and-scale-to-all-decimal-p
		/// 
		/// </summary>
		/// <param name="modelBuilder"></param>
		/// <param name="decimalPrecision"></param>
		public static void SetPrecisionDecimals(this ModelBuilder modelBuilder, KeyValuePair<int, int> decimalPrecision)
		{
			string columnType = $"decimal({decimalPrecision.Key}, {decimalPrecision.Value})"; //"decimal(18, 6)";

			foreach(var property in modelBuilder.Model.GetEntityTypes()
				.SelectMany(t => t.GetProperties())
				.Where(p => p.ClrType == typeof(decimal))) {

				property.SetColumnType(columnType);
				//property.Relational().ColumnType = columnType;
			}
			//modelBuilder.Conventions.Remove<DecimalPropertyConvention>();
			//modelBuilder.Conventions.Add(new System.Data.Entity.ModelConfiguration.Conventions.DecimalPropertyConvention(38, 18));
		}

		public static void SetLog(this DbContextOptionsBuilder optionsBuilder, Action<LogLevel, string> logAction)
		{
			if(logAction != null) {
				optionsBuilder.UseLoggerFactory(DataContextLoggerProvider.CreateFactory(logAction));
				//(state, val) => { Console.WriteLine($" - sql-log: {state}]  {val}"); }));
			}
		}

		//public static void OnModelCreating_SetPrecisionDecimals___EF6(DbModelBuilder modelBuilder)
		//{
		//	modelBuilder.Conventions.Remove<DecimalPropertyConvention>();
		//	modelBuilder.Conventions.Add(new System.Data.Entity.ModelConfiguration.Conventions.DecimalPropertyConvention(38, 18));
		//}
	}
}
