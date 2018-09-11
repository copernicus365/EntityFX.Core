
namespace EntityFX.Core
{
	/// <summary>
	/// The purpose of this type is to allow a user or auto-generated type to inherit this 
	/// interface in order to expose the strongly-typed property names of a SQL table (or whatever else they want). 
	/// This is particularly useful for <see cref="SQLQuery"/>, so that instead of having to
	/// hard-code column names when generating the SQL, you could pass a number of column names through
	/// like `{ t.PersonName, t.PersonAge }`. Since other members here allow EntityFX to auto-generate
	/// a <see cref="ITableDefinition"/> based on your specific tables / DbContexts, it seemed good to define 
	/// this type here, even though on its own it only exposes the <see cref="TableMeta"/>.
	/// </summary>
	public interface ITableDefinition
	{
		TableMetaInfo TableMeta { get; set; }
	}

	public class TableDefinition : ITableDefinition
	{
		public TableMetaInfo TableMeta { get; set; }
	}
}
