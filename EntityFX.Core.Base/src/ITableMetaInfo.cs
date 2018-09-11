
namespace EntityFX.Core
{
	public interface ITableMetaInfoBasic
	{
		string TableNameFull { get; set; }
		string[] TableColumnNames { get; set; }
		string[] EntityPropertyNames { get; set; }
	}

	public interface ITableMetaInfo : ITableMetaInfoBasic
	{
		string TableName { get; set; }
		string TableSchema { get; set; }
		string TypeName { get; set; }
		string TypeNameFull { get; set; }
		string[] KeyColumnNames { get; set; }
	}
}