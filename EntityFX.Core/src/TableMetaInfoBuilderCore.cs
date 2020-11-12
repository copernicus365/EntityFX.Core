using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFX.Core
{
	public class TableMetaInfoBuilderCore : TableMetaInfoBuilderCoreBase
	{
		private readonly DbContext db;

		public TableMetaInfoBuilderCore(DbContext dbContext)
		{
			db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public override IEnumerable<Type> GetEntityTypes()
		{
			Type[] allEntityTypesForThisDbContext = db.Model
				.GetEntityTypes()
				.Select(e => e.ClrType)
				.Where(t => t != null)
				.ToArray();
			return allEntityTypesForThisDbContext;
		}

		public override TableMetaInfo GetTableMetaInfo(Type type)
		{
			IModel model = db.Model;
			IEntityType entityType = model.FindEntityType(type);

			IEntityType[] allEntityTypes = model.GetEntityTypes().ToArray();
			IProperty[] entityProperties = entityType.GetProperties().ToArray();
			EFPropertyInfo[] pProperties = new EFPropertyInfo[entityProperties.Length];

			// Column info 
			for(int i = 0; i < entityProperties.Length; i++) {
				IProperty prop = entityProperties[i];

				string colNm = prop.GetColumnName().NullIfEmptyTrimmed();
				if(colNm == null)
					continue;

				bool isIndex = prop.IsIndex();
				var idx1 = prop.GetContainingKeys().ToArray();

				EFPropertyInfo pInfo = new EFPropertyInfo() {
					EntityName = prop.Name, // prop.Name,
					ColumnName = colNm, //rProp.ColumnName,
					IsIdentity = false,
					IsKey = prop.IsKey(),
					IsIndex = isIndex,
					IsPrimaryKey = prop.IsPrimaryKey()
				};

				string columnType = prop.GetColumnType();

				pProperties[i] = pInfo;
			}

			var info = new TableMetaInfo() {
				TableName = entityType.GetTableName(), // mapping.TableName, //table.Table, // watch out! not table.Name
				TableSchema = entityType.GetSchema(), // mapping.Schema, //table.Schema,
				TypeNameFull = entityType.Name, // or: == type.FullName,
				TypeName = type.Name,
				TableColumnNames = pProperties
					.Select(p => p.ColumnName)
					.Where(n => n.NotNulle())
					.ToArray(), //declaredProps.Select(dp => dp.Name).ToArray(),
				EntityPropertyNames = pProperties
					.Select(p => p.EntityName)
					.Where(n => n.NotNulle())
					.ToArray(), //mapped.Select(dd => dd.Name).ToArray()
				KeyColumnNames = pProperties
					.Where(p => p != null && p.IsKey)
					.Select(p => p.ColumnName)
					.ToArray(), // table.ElementType.KeyMembers.Select(m => m.Name).ToArray(),
			};

			return info;
		}
	}

	class EFPropertyInfo
	{
		public string EntityName { get; set; }
		public string ColumnName { get; set; }
		public bool IsKey { get; set; }
		public bool IsPrimaryKey { get; set; }
		public bool IsIdentity { get; set; }
		public bool IsIndex { get; set; }

		//public bool IsUnique { get; set; }
		//public bool IsDescendingKey { get; set; }
		//public bool IsIncludedColumn { get; set; }
	}
}
