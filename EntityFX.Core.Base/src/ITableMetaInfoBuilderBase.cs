using System;
using System.Collections.Generic;

namespace EntityFX.Core
{
	public interface ITableMetaInfoBuilder
	{
		IEnumerable<Type> GetEntityTypes();

		TableMetaInfo GetTableMetaInfo(Type type);
	}
}
