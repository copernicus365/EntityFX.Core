using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace EntityFX.Core
{
	/// <summary>
	/// Provides a single function (<see cref="ToSql{TEntity}(IQueryable{TEntity})"/>)
	/// allowing one to extract the backing SQL of a EFCore IQueryable.
	/// Source: https://github.com/aspnet/EntityFrameworkCore/issues/6482 
	/// </summary>
	public static class EFCoreSqlGetter
	{
		private static readonly FieldInfo _queryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.Single(x => x.Name == "_queryCompiler");

		private static readonly TypeInfo _queryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

		private static readonly FieldInfo _queryModelGeneratorField = _queryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_queryModelGenerator");

		private static readonly FieldInfo _databaseField = _queryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");

		private static readonly PropertyInfo _dependenciesProperty = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

		public static string ToSql<TEntity>(this IQueryable<TEntity> queryable)
			where TEntity : class
		{
			if (!(queryable is EntityQueryable<TEntity>) && !(queryable is InternalDbSet<TEntity>))
				throw new ArgumentException();

			var queryCompiler = (IQueryCompiler)_queryCompilerField.GetValue(queryable.Provider);
			var queryModelGenerator = (IQueryModelGenerator)_queryModelGeneratorField.GetValue(queryCompiler);
			var queryModel = queryModelGenerator.ParseQuery(queryable.Expression);
			var database = _databaseField.GetValue(queryCompiler);
			var queryCompilationContextFactory = ((DatabaseDependencies)_dependenciesProperty.GetValue(database)).QueryCompilationContextFactory;
			var queryCompilationContext = queryCompilationContextFactory.Create(false);
			var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
			modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
			return modelVisitor.Queries.Join(Environment.NewLine + Environment.NewLine);
		}
	}

	#region --- Second way to get IQueryable.ToSql: Pomelo.EntityFrameworkCore.Extensions.ToSql ---

	//// the following 
	//using Remotion.Linq;
	//using Remotion.Linq.Parsing.Structure;
	// 
	///// <summary>
	///// https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.Extensions.ToSql/blob/master/Pomelo.EntityFrameworkCore.Extensions.ToSql/IQueryableExtensions.cs
	///// 
	///// </summary>
	//public static class EFCoreSqlGetter2
	//{
	//	public static string ToSql2<TEntity>(this IQueryable<TEntity> self)
	//	{
	//		var visitor = self.CompileQuery();
	//		return string.Join("", visitor.Queries.Select(x => x.ToString().TrimEnd().TrimEnd(';') + ";" + Environment.NewLine));
	//	}

	//	public static IEnumerable<string> ToUnevaluated<TEntity>(this IQueryable<TEntity> self)
	//	{
	//		var visitor = self.CompileQuery();
	//		return VisitExpression(visitor.Expression, null);
	//	}

	//	internal static IEnumerable<string> VisitExpression(Expression expression, dynamic caller)
	//	{
	//		var ret = new List<string>();
	//		dynamic exp = expression;

	//		if (expression.NodeType == ExpressionType.Lambda) {
	//			var resultBuilder = new StringBuilder();
	//			if (caller != null) {
	//				resultBuilder.Append(caller.Method.Name.Replace("_", "."));
	//				resultBuilder.Append("(");
	//			}
	//			resultBuilder.Append(exp.ToString());

	//			if (caller != null) {
	//				resultBuilder.Append(")");
	//			}
	//			ret.Add(resultBuilder.ToString());
	//		}

	//		try {
	//			if (exp.Arguments.Count > 0) {
	//				foreach (var x in exp.Arguments) {
	//					ret.AddRange(VisitExpression(x, exp));
	//				}
	//			}
	//		}
	//		catch { }

	//		return ret;
	//	}

	//	public static class ReflectionCommon
	//	{
	//		public static readonly FieldInfo QueryCompilerOfEntityQueryProvider = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
	//		public static readonly PropertyInfo DatabaseOfQueryCompiler = typeof(QueryCompiler).GetTypeInfo().DeclaredProperties.First(x => x.Name == "Database");
	//		public static readonly PropertyInfo DependenciesOfDatabase = typeof(Database).GetTypeInfo().DeclaredProperties.First(x => x.Name == "Dependencies");
	//		public static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
	//		public static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");
	//		public static readonly PropertyInfo NodeTypeProvider = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");
	//		public static readonly PropertyInfo QueriesOfRelationalQueryModelVisitor = typeof(RelationalQueryModelVisitor).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Queries");
	//	}

	//	public static RelationalQueryModelVisitor CompileQuery<TEntity>(this IQueryable<TEntity> self)
	//	{
	//		var q = self as EntityQueryable<TEntity>;
	//		if (q == null) {
	//			return null;
	//		}
	//		var fields = typeof(Database).GetTypeInfo().DeclaredFields;

	//		var queryCompiler = (QueryCompiler)ReflectionCommon.QueryCompilerOfEntityQueryProvider.GetValue(self.Provider);
	//		var database = (Database)ReflectionCommon.DatabaseOfQueryCompiler.GetValue(queryCompiler);
	//		var dependencies = (DatabaseDependencies)ReflectionCommon.DependenciesOfDatabase.GetValue(database);
	//		var factory = dependencies.QueryCompilationContextFactory;
	//		var nodeTypeProvider = (INodeTypeProvider)ReflectionCommon.NodeTypeProvider.GetValue(queryCompiler);
	//		var parser = (QueryParser)ReflectionCommon.CreateQueryParserMethod.Invoke(queryCompiler, new object[] { nodeTypeProvider });
	//		var queryModel = parser.GetParsedQuery(self.Expression);
	//		var modelVisitor = (RelationalQueryModelVisitor)database.CreateVisitor(factory, queryModel);
	//		modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
	//		return modelVisitor;
	//	}


	//	public static EntityQueryModelVisitor CreateVisitor(this Database self, IQueryCompilationContextFactory factory, QueryModel qm)
	//	{
	//		return factory.Create(async: false).CreateQueryModelVisitor();
	//	}
	//}

	#endregion

}
