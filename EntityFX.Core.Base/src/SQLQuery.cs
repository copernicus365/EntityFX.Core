using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFX.Core
{
	public class SQLQuery : ITableMetaInfoBasic
	{
		public SQLQuery() { }

		public SQLQuery(
			ITableMetaInfoBasic tableMetaInfo,
			string orderByClause)
		{
			TableNameFull = tableMetaInfo.TableNameFull;
			TableColumnNames = tableMetaInfo.TableColumnNames.ToArray();
			EntityPropertyNames = tableMetaInfo.EntityPropertyNames.ToArray();
			OrderByClause = orderByClause;
		}



		#region SIMPLE PROPERTIES

		static ushort _argsIncr;

		public string TableNameFull { get; set; }
		public string[] TableColumnNames { get; set; }
		public string[] EntityPropertyNames { get; set; }
		public string OrderByClause { get; set; }

		public string SelectVal { get; set; }
		public int CountVal { get; set; }
		public int StartVal { get; set; }

		public SQLQuery NextQueryVal { get; set; }
		public List<string> SetClauses = new List<string>();
		public List<string> WhereClauses = new List<string>();
		public List<SqlParam> Args = new List<SqlParam>();

		#endregion



		public SQLQuery NextQuery(SQLQuery nextQuery)
		{
			this.NextQueryVal = nextQuery;
			return this;
		}



		#region RANGE

		public SQLQuery Start(int start)
		{
			StartVal = start;
			return this;
		}

		public SQLQuery Count(int count)
		{
			CountVal = count;
			return this;
		}

		public SQLQuery Range(int start, int count)
		{
			StartVal = start;
			CountVal = count;
			return this;
		}

		#endregion



		#region SELECT

		public SQLQuery SelectA(params string[] selects)
		{
			return SelectA((IEnumerable<string>)selects);
		}

		public SQLQuery SelectA(IEnumerable<string> selects)
		{
			SelectVal = selects.Where(s => s.NotNulle()).JoinToString(",\r\n");
			return this;
		}

		public SQLQuery SelectClause(string selectClause = "*")
		{
			SelectVal = selectClause;
			return this;
		}

		public SQLQuery SelectAll()
		{
			if(TableColumnNames.NotNulle())
				return Select(new SelectColumns(this));
			return SelectClause("*");
		}

		public SQLQuery SelectAllExcept(params string[] exceptCols)
		{
			if(TableColumnNames.NotNulle())
				return Select(new SelectColumns(this).Except(exceptCols));
			return SelectClause("*");
		}

		public SQLQuery Select(SelectColumns selectColumns)
		{
			SelectVal = selectColumns.ToSelects().JoinToString(", \r\n");
			return this;
		}

		//public SQLQuery SelectAllButNullifyThisColumn(string columnNameToNull)
		//{
		//	return this
		//		.Select(new SelectColumns(this).NullifyColumns(columnNameToNull)); // this.TabelDef.SelectColumns()
		//		//.Replace(columnNameToNull, "NULL AS " + columnNameToNull));
		//}

		public SQLQuery SelectAllButNullifyTheseColumns(params string[] cols)
		{
			return this
				.Select(
					new SelectColumns(this)
					.NullifyColumns(cols));
			//.Replace(columnNameToNull, "NULL AS " + columnNameToNull));
		}

		#endregion



		public SQLQuery IfThen(bool condition, Func<SQLQuery, SQLQuery> func)
		{
			if(condition)
				return func(this);
			return this;
		}

		/// <summary>
		/// Sets the OrderBy clause to be used in the query.
		/// </summary>
		public SQLQuery OrderBy(string orderByClause)
		{
			this.OrderByClause = orderByClause?.Trim();
			return this;
		}

		/// <summary>
		/// Sets the full table name.
		/// </summary>
		public SQLQuery TableName(string tableName)
		{
			TableNameFull = tableName;
			return this;
		}



		#region WHERE

		public SQLQuery Where(string whereClause)
		{
			_AddWhereClause(whereClause);
			return this;
		}

		public SQLQuery WhereEquals(string name, object value)
		{
			return Where(name, value, "=");
		}

		public SQLQuery WhereIn<T>(string name, params T[] values)
		{
			string val = "(" + values.JoinToString(",") + ")";
			return Where(name, val, "IN");
		}

		public SQLQuery Where(string name, object value, string operatr = "=")
		{
			return __WhereOrSet(true, name, value, operatr);
		}

		// ---

		public SQLQuery IfWhere(bool condition, string whereClause)
		{
			_AddWhereClause(whereClause);
			return !condition ? this : Where(whereClause);
		}

		public SQLQuery IfWhereEquals(bool condition, string name, object value)
		{
			return !condition ? this : Where(name, value, "=");
		}

		public SQLQuery IfWhere(bool condition, string name, object value, string operatr = "=")
		{
			return !condition ? this : Where(name, value, operatr);
		}






		public SQLQuery Set(string setClause)
		{
			_AddSetClause(setClause);
			return this;
		}

		public SQLQuery SetEquals(string name, object value)
		{
			return Set(name, value, "=");
		}

		public SQLQuery Set(string name, object value, string operatr = "=")
		{
			return __WhereOrSet(false, name, value, operatr);
		}

		SQLQuery __WhereOrSet(bool isWhere, string name, object value, string operatr = "=")
		{
			name = name.Trim();
			if(name[0] == '@')
				name = name.Substring(1);

			string argName = string.Format("@{0}_{1}", _argsIncr++, name);

			if(value == null)
				value = DBNull.Value;

			if(operatr.Equals("IN", StringComparison.OrdinalIgnoreCase)) {
				argName = value.ToString();
			}
			else
				Args.Add(new SqlParam(argName, value));

			string cls = string.Format("{0} {1} {2}", name, operatr, argName);
			if(isWhere)
				_AddWhereClause(cls);
			else
				_AddSetClause(cls);
			return this;
		}

		public string GetWhereClause {
			get {
				if(WhereClauses.IsNulle())
					return null;
				string whr = WhereClauses.JoinToString(" AND ");
				return whr;
			}
		}

		public string GetSetClause {
			get {
				if(SetClauses.IsNulle())
					return null;
				return SetClauses.JoinToString(",\r\n");
			}
		}

		void _AddWhereClause(string w, bool prep = true)
		{
			if(prep)
				w = _PrepWhereOrSet(w, "WHERE");
			if(w.NotNulle())
				WhereClauses.Add(w);
		}

		void _AddSetClause(string s, bool prep = true)
		{
			if(prep)
				s = _PrepWhereOrSet(s, "SET");
			if(s.NotNulle())
				SetClauses.Add(s);
		}

		string _PrepWhereOrSet(string s, string baseWord)
		{
			if(s != null)
				s = s.Trim();
			if(s.IsNulle())
				return null;

			int len = baseWord.Length;
			if(s.StartsWith(baseWord, StringComparison.OrdinalIgnoreCase) && s.Length > len && char.IsWhiteSpace(s[len]))
				s = s.Substring(len + 1).Trim();
			return s;
		}

		#endregion



		public string ToSQL()
		{
			string val = ToSQL(out SqlParam[] sqlArgs);
			if(sqlArgs.NotNulle())
				throw new ArgumentNullException("SqlParameters were set as an out parameter, which means you must call the overload that accepts the out SqlParam[] sqlArgs.");
			return val;
		}

		public string ToSQL(out SqlParam[] sqlArgs)
		{
			bool hasRange = StartVal > 0 || CountVal > 0;

			OrderByClause = OrderByClause.NullIfEmptyTrimmed();

			//if (OrderByVal.IsNulle()) // OLD <-- keep for moment
			//  OrderByVal = TabelDef?.GroupByClause; 
			//  OrderByVal = TabelDef.GroupByClause; //was: tableDefqqq.TableInfo.IndexInfo.OrderByClause;

			if(TableNameFull.IsNulle())
				throw new ArgumentNullException("tableName");

			if(hasRange && OrderByClause.IsNulle())
				throw new ArgumentNullException("OrderBy clause required in Range query.");

			sqlArgs = GetSqlParameters.ToArray();
			if(sqlArgs.IsNulle()) sqlArgs = null;

			// SELECT
			var sb = new StringBuilder();

			if(SelectVal.NotNulle()) {
				sb.AppendFormat(
	@"SELECT {0}
FROM {1}", SelectVal, TableNameFull);
			}
			else if(SetClauses.NotNulle()) {
				sb.AppendFormat(
@"UPDATE {0}
SET {1}", TableNameFull, GetSetClause);
			}
			else
				throw new ArgumentNullException("SQL action, such as SELECT or UPDATE, must be specified.");

			sb.AppendLine();

			// WHERE
			string whr = GetWhereClause;
			if(whr.NotNulle()) {
				sb.Append(
@"WHERE ");
				sb.AppendLine(whr);
			}

			// ORDERBY
			if(SelectVal.NotNulle()) {
				if(hasRange && OrderByClause.IsNulle()) {
					throw new ArgumentNullException($"SQL range query requires {OrderByClause} to be set.");
					// NEW, didn't throw on this prior to 2018-05-24, not sure why we didn't, 
					// but just in case this shouldn't throw... this note
				}

				if(OrderByClause.NotNulle()) {
					sb.Append(
	@"ORDER BY ");
					sb.AppendLine(OrderByClause);
				}
			}

			// RANGE
			if(hasRange) {
				sb.AppendLine("OFFSET @Offset ROWS FETCH FIRST @Count ROWS ONLY");
			}

			if(NextQueryVal != null) {

				sb.TrimEnd();
				sb.AppendLine(";\r\n");

				sb.AppendLine(NextQueryVal.ToSQL(out SqlParam[] sqlArgs2));
				sqlArgs = sqlArgs.JoinSequences(sqlArgs2).ToArray();

				sb.TrimEnd();
				if(sb[sb.Length - 1] != ';')
					sb.AppendLine(";\r\n");
			}

			string result = sb.ToString();
			return result;
		}

		public IEnumerable<SqlParam> GetSqlParameters {
			get {
				if(StartVal > 0 || CountVal > 0) {
					yield return new SqlParam("@Offset", StartVal);
					yield return new SqlParam("@Count", CountVal);
				}
				if(Args.NotNulle()) {
					foreach(var arg in Args)
						yield return arg;
				}
			}
		}

	}
}
