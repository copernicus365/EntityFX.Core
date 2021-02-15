using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace EntityFX.Core
{
	/// <summary>
	/// Provides a single function (<see cref="ToSql{TEntity}(IQueryable{TEntity})"/>)
	/// allowing one to extract the backing SQL of a EFCore IQueryable.
	/// Source: https://github.com/aspnet/EntityFrameworkCore/issues/6482 
	/// </summary>
	public static class EFCoreSqlGetter
	{
		private static object Private(this object obj, string privateField) => obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
		private static T Private<T>(this object obj, string privateField) => (T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);

		/// <summary>
		/// Source: https://stackoverflow.com/a/51583047/264031
		/// </summary>
		public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
		{
			var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
			var relationalCommandCache = enumerator.Private("_relationalCommandCache");
			var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
			var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

			var sqlGenerator = factory.Create();
			var command = sqlGenerator.GetCommand(selectExpression);

			string sql = command.CommandText;
			return sql;
		}
	}
}
