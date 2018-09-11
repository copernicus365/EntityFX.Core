using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EntityFX.Core
{
	public class SelectColumns
	{
		// would set these as readonly if possible (indexing into them changes), but usage below should never alter these
		string[] _TableColumnNames { get; set; }
		string[] _EntityPropertyNames { get; set; }


		public Dictionary<string, bool> ExceptCols;
		public Dictionary<string, string> ReplaceCols;
		public List<string> Adds;

		public static readonly Regex rxHasAS = new Regex(@"\sAS\s", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

		public SelectColumns(ITableMetaInfoBasic t)
		{
			if (t == null) throw new ArgumentNullException(nameof(t));
			_TableColumnNames = t.TableColumnNames;
			_EntityPropertyNames = t.EntityPropertyNames;
		}

		public SelectColumns(string[] tableColNames, string[] entityPropNames)
		{
			_TableColumnNames = tableColNames;
			_EntityPropertyNames = entityPropNames;
		}

		#region EXCEPT

		public SelectColumns __Except(IEnumerable<string> except)
		{
			if (except != null) {

				if(ExceptCols == null)
					ExceptCols = new Dictionary<string, bool>();

				foreach(string val in except) {
					if(val.NotNulle() && !ExceptCols.ContainsKey(val))
						ExceptCols[val] = false;
				}
			}
			return this;
		}

		public SelectColumns Except(IEnumerable<string> except)
		{
			return __Except(except);
		}
		public SelectColumns Except(params string[] except)
		{
			return except.IsNulle() ? this : __Except(except);
		}
		public SelectColumns ExceptIf(bool condition, params string[] except)
		{
			return condition && except.NotNulle()
				? __Except(except)
				: this;
		}
		public SelectColumns ExceptIf(bool condition, IEnumerable<string> except)
		{
			return condition
				? __Except(except)
				: this;
		}

		#endregion

		#region Replace

		public SelectColumns NullifyColumns(params string[] cols)
		{
			if (cols.NotNulle()) {
				foreach (string col in cols) {
					if (col.IsNulle())
						throw new ArgumentNullException();

					if (ReplaceCols == null)
						ReplaceCols = new Dictionary<string, string>();
					ReplaceCols[col] = "NULL AS " + col;
				}
			}
			return this;
		}

		public SelectColumns Replace(string match, string replace)
		{
			if (match != null) {
				if (replace.IsNulle()) throw new ArgumentNullException();
				if (ReplaceCols == null) ReplaceCols = new Dictionary<string, string>();
				ReplaceCols[match] = replace;
			}
			return this;
		}
		public SelectColumns ReplaceIf(bool condition, string match, string replace)
		{
			return condition
				? Replace(match, replace)
				: this;
		}

		#endregion

		#region Add

		public SelectColumns Add(string select)
		{
			if (select.NotNulle()) {
				if (Adds == null) Adds = new List<string>();
				Adds.Add(select);
			}
			return this;
		}

		#endregion

		bool hasAs(string val)
		{
			if (val.IsNulle()) return false;
			return rxHasAS.Match(val).Success;
		}

		public string[] ToSelects()
		{
			string[] cols = _TableColumnNames.ToArray();
			string[] ecols = _EntityPropertyNames.ToArray();

			bool exceptEmpty = ExceptCols.IsNulle();
			bool replaceEmpty = ReplaceCols.IsNulle();

			string c = null;
			if (!exceptEmpty || !replaceEmpty) {
				for (int i = 0; i < cols.Length; i++) {
					c = cols[i];
					if (!exceptEmpty && ExceptCols.ContainsKey(c))
						cols[i] = null;
					else if (!replaceEmpty && ReplaceCols.ContainsKey(c)) {
						cols[i] = ReplaceCols[c];
					}
				}
			}

			// STEP 2

			for (int i = 0; i < cols.Length; i++) {
				c = cols[i];
				if (c != null) {
					if (ecols[i] != null && !hasAs(c)) {
						if (c == _TableColumnNames[i]) // removed! Not needed! `cols` came from TableColumns, 
							c = $"[{c}]"; //"[" + c + "]";
						cols[i] = $"{c} AS [{ecols[i]}]"; // c + " AS [" + ecols[i] + "]";
					}
				}
			}

			var vals = cols.Where(v => v.NotNulle());
			if (Adds.NotNulle())
				vals = vals.Concat(Adds);
			string[] final = vals.ToArray();
			return final;
		}
	}
}
