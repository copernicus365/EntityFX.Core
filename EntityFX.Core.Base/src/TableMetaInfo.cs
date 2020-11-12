using System;
using System.Linq;

namespace EntityFX.Core
{
	public class TableMetaInfo : ITableMetaInfo
	{
		public string TypeName { get; set; }

		public string TypeNameFull { get; set; }

		public string[] TableColumnNames { get; set; }

		public string[] EntityPropertyNames { get; set; }

		public string[] KeyColumnNames { get; set; }




		public string TableName {
			get => _TableName;
			set {
				_TableFullName = null;
				_TableName = value;
			}
		}
		string _TableName;

		public string TableSchema {
			get => _TableSchema;
			set {
				_TableFullName = null;
				_TableSchema = value;
			}
		}
		string _TableSchema;

		/// <summary>
		/// If null, this property is auto-constructed from <see cref="TableName"/> and
		/// <see cref="TableSchema"/> (though cached, until if or when one of the former two change).
		/// </summary>
		public string TableNameFull {
			get {
				if(_TableFullName.IsNulle()) {
					_TableFullName = GetFullTableName(TableSchema, TableName);
				}
				return _TableFullName;
			}
			// since this is a constructed value, could do without setter, but ITableMetaInfo
			// is for more purposes where a setter is needed, for now we'll just set.
			set => _TableFullName = value;
		}
		string _TableFullName;

		static string _bracket(string val)
		{
			return val.IsNulle() || val[0] == '['
				? val
				: '[' + val + ']';
		}

		public static string GetFullTableName(string schema, string name)
		{
			if(schema.IsNulle()) {
				return name.IsNulle() ? null : _bracket(name);
			}
			if(schema.NotNulle() && name.IsNulle())
				throw new ArgumentNullException("The table name must be set if the schema is specified.");

			return _bracket(schema) + '.' + _bracket(name);
		}

		public TableMetaInfo Copy()
		{
			var tmi = MemberwiseClone() as TableMetaInfo;
			tmi.TableColumnNames = this.TableColumnNames.ToArray();
			tmi.EntityPropertyNames = this.EntityPropertyNames.ToArray();
			tmi.KeyColumnNames = this.KeyColumnNames.ToArray();
			return tmi;
		}
	}
}
