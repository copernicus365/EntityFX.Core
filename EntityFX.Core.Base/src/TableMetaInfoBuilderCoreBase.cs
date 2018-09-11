using System;
using System.Collections.Generic;

namespace EntityFX.Core
{
	public abstract class TableMetaInfoBuilderCoreBase : ITableMetaInfoBuilder
	{
		public TableMetaInfoBuilderCoreBase() { }

		public abstract TableMetaInfo GetTableMetaInfo(Type type);

		public abstract IEnumerable<Type> GetEntityTypes();

	}
}
