using System;

namespace EntityFX.Core
{
	/// <summary>
	/// Attribute for entity classes whose table is a primary composite key.
	/// This attribute can be marked on the class level of that entity class, 
	/// which can be used by consumers (such as by DbRepositoryCore) for knowing the
	/// normal sort order of that table. This is particularly needed because unfortunately
	/// some SQL Server queries, in particular the offset + row fetch pagination-like
	/// query (`OFFSET @Offset ROWS FETCH FIRST @Count ROWS ONLY`) requires an ORDERBY clause 
	/// to be specified, instead of just using the default order of the table.
	/// <para />
	/// For analysis of other attributes, see: 
	/// https://github.com/dotnet/corefx/blob/master/src/System.ComponentModel.Annotations/src/System/ComponentModel/DataAnnotations/Schema/TableAttribute.cs
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class PKOrderAttribute : Attribute
	{
		private string _pkOrderClause;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="pkOrder">The value.</param>
		public PKOrderAttribute(string pkOrder)
		{
			PKOrder = pkOrder;
		}

		/// <summary>
		/// The composite primary key order value, e.g. `GroupName ASC, Kind, Id DESC`.
		/// </summary>
		public string PKOrder {
			get => _pkOrderClause;
			set {
				string val = value.NullIfEmptyTrimmed();
				if (val.IsNullOrWhiteSpace())
					throw new ArgumentException("Value is null or white space", nameof(value));
				_pkOrderClause = val;
			}
		}
	}
}
