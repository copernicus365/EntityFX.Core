using System;

using EntityFX.Core;

using Test.EntityFX.Core.Mock;

using Xunit;

namespace Test.EntityFX.Core
{
	public class TableInfoTests
	{
		public const string DbConn_InMemoryCoolDbName = "InMemoryCoolDbName";

		public static CoolDatabaseDbx GetInMemoryCoolDatabaseDbx()
			=> new CoolDatabaseDbx() { InMemoryDatabaseName = DbConn_InMemoryCoolDbName };

		public static WidgetRepo GetInMemoryWidgetRepo()
			=> new WidgetRepo(GetInMemoryCoolDatabaseDbx());

		[Fact]
		public void IntegrationTest_GetFullTableInfoFromRepo1()
		{
			WidgetRepo repo = GetInMemoryWidgetRepo();

			TableMetaInfo tinfo = repo.GetNewTableInfo();

			Assert.True(tinfo.TableName != null);
		}
	}
}
