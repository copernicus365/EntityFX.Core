using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EntityFX.Core
{
	public interface IDbRepositoryCore<T, TId> : IDbRepositoryCoreTD<T, TId, TableDefinition>
		where T : class
	{
	}

	public interface IDbRepositoryCoreTD<T, TId, TTableDef> : IDbRepositoryCoreTD
		where T : class
		where TTableDef : ITableDefinition, new()
	{
		TTableDef t { get; }
		void Add(T entity);
		int Count(Expression<Func<T, bool>> predicate = null);
		Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
		void Delete(T entity);
		int Delete(TId id, bool? deleteDirect = null);
		int DeleteDirect(TId id);
		int DeleteDirect(TId id, NameValueMatch nvm);
		Task<int> DeleteDirectAsync(TId id);
		Task<int> DeleteDirectAsync(TId id, NameValueMatch nvm);
		int ExecuteProc(string procName, TId id, params KeyValuePair<string, object>[] args);
		int ExecuteProc(string procName, TId id, params SqlParam[] args);
		Task<int> ExecuteProcAsync(string procName, TId id, params KeyValuePair<string, object>[] args);
		Task<int> ExecuteProcAsync(string procName, TId id, params SqlParam[] args);
		IQueryable<T> Get(bool? noTracking = null);
		T GetById(TId id, bool? noTracking = null);
		Task<T> GetByIdAsync(TId id, bool? noTracking = null);
		IQueryable<T> GetRange(Expression<Func<T, bool>> predicate, int index, int take, bool? noTracking = null);
		IQueryable<T> GetRange(int index, int take, bool? noTracking = null);
		IQueryable<T> GetRange(IQueryable<T> source, Expression<Func<T, bool>> predicate, int index, int take);
		IQueryable<T> GetRange(IQueryable<T> source, int index, int take);
		IQueryable<T> GetTest(TId xyz);
		bool IdNotSet(T item);
		string IdToString(TId id);
		bool MatchesId(T item1, T item2);
		IOrderedQueryable<T> PrimaryOrder(IQueryable<T> source);
		void Update(T entity);
		void Update(T entity, params Expression<Func<T, object>>[] properties);
		int UpdateDirect(TId id, Func<TTableDef, IEnumerable<KeyValuePair<string, object>>> exp);
		int UpdateDirect(TId id, Func<TTableDef, KeyValuePair<string, object>> exp);
		int UpdateDirect(TId id, Func<TTableDef, string> exp, object value);
		int UpdateDirect(TId id, params KeyValuePair<string, object>[] vals);
		int UpdateDirect(TId id, params KV[] vals);
		int UpdateDirect(TId id, params SqlParam[] updateColumns);
		int UpdateDirect(TId id, string updateColumnName, object value);
		Task<int> UpdateDirectAsync(TId id, Func<TTableDef, IEnumerable<KeyValuePair<string, object>>> exp);
		Task<int> UpdateDirectAsync(TId id, Func<TTableDef, KeyValuePair<string, object>> exp);
		Task<int> UpdateDirectAsync(TId id, Func<TTableDef, string> exp, object value);
		Task<int> UpdateDirectAsync(TId id, params KeyValuePair<string, object>[] vals);
		Task<int> UpdateDirectAsync(TId id, params KV[] vals);
		Task<int> UpdateDirectAsync(TId id, params SqlParam[] updateColumns);
		Task<int> UpdateDirectAsync(TId id, string updateColumnName, object value);
		void Upsert(T entity);
		IQueryable<T> WhereMatchTId(IQueryable<T> source, TId id);
	}

	public interface IDbRepositoryCoreTD : IDisposable
	{
		bool AsNoTracking { get; }
		bool DeleteDirectDefault { get; }
		string FullTableName { get; }
		string IdName { get; set; }
		string PKOrder { get; set; }
		string PKOrderFinal { get; }
		TableMetaInfo TableMeta { get; }

		/// <summary>
		/// Gets a new instance of <see cref="SQLQuery"/>, set with this repo's meta info.
		/// </summary>
		SQLQuery GetSQLQuery();

		int DeleteAll();
		int DeleteDirect(NameValueMatch nvm);
		int DeleteDirectWhere(string colName, object value, string operatr = "=");
		Task<int> DeleteDirectWhereAsync(string colName, object value, string operatr = "=");
		int ExecuteProc(string procName, params SqlParam[] args);
		Task<int> ExecuteProcAsync(string procName, params SqlParam[] args);
		TableMetaInfo GetNewTableInfo();
		int SaveChanges();
		Task<int> SaveChangesAsync();
		Task<int> SetIdentityInsert(bool turnOn);
		int UpdateDirect(params SqlParam[] updateColumns);
		int UpdateDirect(string whereClause, params SqlParam[] updateColumns);
		Task<int> UpdateDirectAsync(params SqlParam[] updateColumns);
		Task<int> UpdateDirectAsync(string whereClause, params SqlParam[] updateColumns);

		string GetTableDefinitionsCode(
			bool allDbSets,
			TDCodeGenerator codeGenOps = null,
			bool withNamespace = true);
	}
}
