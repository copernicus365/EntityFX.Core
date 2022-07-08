using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFX.Core
{
	public abstract class DbRepositoryCore<T, TId> :
		DbRepositoryCore<T, TId, TableDefinition>,
		IDbRepositoryCore<T, TId>
		where T : class
	{
		public DbRepositoryCore(DbContext dbContext) : base(dbContext) { }
	}

	public abstract class DbRepositoryCore<T, TId, TTableDef> :
		IDbRepositoryCoreTD<T, TId, TTableDef> where T : class
		where TTableDef : ITableDefinition, new()
	{
		// --- FIELDS ---

		protected DbContext _dbContext;
		protected DbSet<T> _dbSet;
		protected DatabaseFacade _db;

		// === STATIC FIELDS ===

		protected static string _tableName;
		protected static TId _defaultId = default;
		protected static TTableDef _t = new TTableDef();

		static bool _static_orderByAttrSet;
		static string _static_orderByAttrVal;

		// --- Properties ---

		//public TTableDef t => _t;
		public virtual TTableDef t { get; } = _t;

		protected static TableMetaInfo _TableInfo;
		protected static TableDefinition TD;

		static void SetTabelInfo(TableMetaInfo tableMetaInfo)
		{
			_TableInfo = tableMetaInfo;
			_tableName = _TableInfo.TableNameFull;

		}

		/// <summary>
		/// TableInfo, is backed by a STATIC field for this repo generic type,
		/// so this does NOT set an instance field per type, and upon being called
		/// the first time, it is only set once for any of a given repo instance type.
		/// Internally, TableInfo is gotten via an extension method on DbContext, which
		/// uses the new EF 6.1 meta-data:
		/// <para/>
		/// <code>dbcontext.GetTableMetaInfo(typeof(T))</code>
		/// </summary>
		public TableMetaInfo TableMeta {
			get {
				_TableInfo = GetNewTableInfo();
				// note this is STATIC! will only need set ONCE for this generic DbRepo child type
				if(_TableInfo == null)
					SetTabelInfo(GetNewTableInfo());
				return _TableInfo;
			}
		}

		/// <summary>
		/// Returns an instance of 
		/// </summary>
		public TDCodeGenerator Code => new TDCodeGenerator() { };

		public string FullTableName => TableMeta.TableNameFull;


		public DbRepositoryCore(DbContext dbContext)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_dbSet = _dbContext.Set<T>();
			_db = _dbContext.Database;
			//context.Configuration.ValidateOnSaveEnabled = settings.ValidateOnSaveEnabled;

			INIT();
		}

		protected void INIT()
		{
			// FIRST TIME STATIC SET
			if(!_static_orderByAttrSet) {
				_static_orderByAttrSet = true;

				if(typeof(T).GetCustomAttributes(typeof(PKOrderAttribute), inherit: true).FirstOrDefault() is PKOrderAttribute att)
					_static_orderByAttrVal = att.PKOrder.NullIfEmptyTrimmed();
			}

			// OrderByClauseFinal
			if(_static_orderByAttrVal.NotNulle() && PKOrder.IsNulle())
				PKOrder = _static_orderByAttrVal.ToString();

			if(_t.TableMeta == null) {
				_t.TableMeta = TableMeta.Copy();
			}
		}

		#region --- Abstract members (and any virtual members as prime candidates to consider implementing)  ---

		public abstract IQueryable<T> WhereMatchTId(IQueryable<T> source, TId id);

		public abstract bool IdNotSet(T item);

		public abstract bool MatchesId(T item1, T item2);

		public abstract IOrderedQueryable<T> PrimaryOrder(IQueryable<T> source); // toOrderedQueryable

		/// <summary>
		/// Convert TId to string. Override to do something more than just ToString default.
		/// Note, this is NOT called within queries, only on plain, local CLR objects, so no worries about ruining a query.
		/// </summary>
		public virtual string IdToString(TId id) => id?.ToString();

		public virtual string IdName { get; set; } = "Id";

		public virtual string PKOrder { get; set; }

		/// <summary>
		/// Returns <see cref="PKOrder"/> if not null, else <see cref="IdName"/>.
		/// If both are null, null will be returned.
		/// </summary>
		public virtual string PKOrderFinal
			=> PKOrder ?? IdName;

		/// <summary>
		/// Indicates a direct database call, using DbRepository's DeleteDirect,
		/// should be made for Delete functions by default, instead of
		/// calling EF's way that requires having to load the object first
		/// from db, a needless double hit to the db in those cases.
		/// TRUE by default.
		/// </summary>
		public virtual bool DeleteDirectDefault => true;

		/// <summary>
		/// The default value is set to the static value of <see cref="AsNoTrackingDefault"/>.
		/// (currently is false). Set to false (default) gives the expected and typical EF behavior,
		/// so that gotten items ARE cached in the dbContext.
		/// But when set to TRUE, calling `Get(bool?)` with no parameters
		/// (returning dbset essentially) returns dbset.AsNoTracking(), which also effects GetById 
		/// or any other of the many
		/// code calls that start with Get(). Note that there is an overload on `Get(bool?)`
		/// allowing you to specify whether to track or not still.
		/// <para />
		/// The documentation for AsNoTracking is as follows:
		/// <para/>
		/// Returns a new query where the entities returned will not be cached in the
		/// System.Data.Entity.DbContext.
		/// </summary>
		public virtual bool AsNoTracking => AsNoTrackingDefault;

		public static bool AsNoTrackingDefault = false;


		#endregion


		#region --- GET ---

		public virtual T GetById(TId id, bool? noTracking = null)
		{
			bool _asNoTracking = noTracking ?? AsNoTracking; // _dbSettings.AsNoTracking;
															 //if (!_asNoTracking)
															 //	return _dbSet.Find(id);
			return WhereMatchTId(Get(_asNoTracking), id).FirstOrDefault(); // .FirstOrDefault(e => _getIdFromT(e).Equals(id));
																		   //e.Id.Equals(id));
																		   //PredicateBuilder.GetByIdPredicate<T>(id));
																		   //return dbset.FirstOrDefault(PredicateBuilder.GetByIdPredicate<T>(id));
																		   //return dbset.Find(id);
		}

		public virtual async Task<T> GetByIdAsync(TId id, bool? noTracking = null)
		{
			bool _asNoTracking = noTracking ?? AsNoTracking; // _dbSettings.AsNoTracking;
															 //if (!_asNoTracking)
															 //	return await _dbSet.FindAsync(id);
			return await WhereMatchTId(Get(_asNoTracking), id).FirstOrDefaultAsync(); //Get(_asNoTracking).FirstOrDefaultAsync(e => _getIdFromT(e).Equals(id)); //e.Id.Equals(id));
		}

		public virtual IQueryable<T> Get(bool? noTracking = null)
		{
			bool _asNoTracking = noTracking ?? AsNoTracking; // _dbSettings.AsNoTracking;
			return _asNoTracking
				? _dbSet.AsNoTracking()
				: _dbSet;
		}

		public virtual IQueryable<T> GetTest(TId xyz)
		{
			var q = WhereMatchTId(Get(), xyz); //.Where(e => _getIdFromT(e).Equals(xyz));
											   //var q = Get().Where(e => _getIdFromT(e).Equals(xyz));
											   //var z = Get().Where(e => e _getIdFromT(e).Equals(xyz));

			//q = Get().Where(_tMatch);
			return q;
		}

		#endregion

		public IQueryable<T> GetRange(
			int index,
			int take,
			bool? noTracking = null)
			=> GetRange(Get(noTracking), index, take);

		public IQueryable<T> GetRange(
			IQueryable<T> source,
			int index,
			int take)
		{
			var q = PrimaryOrder(source)
				.Skip(index)
				.Take(take);
			return q;
		}

		public IQueryable<T> GetRange(
			Expression<Func<T, bool>> predicate,
			int index,
			int take,
			bool? noTracking = null)
			=> GetRange(Get(noTracking), predicate, index, take);

		public IQueryable<T> GetRange(
			IQueryable<T> source,
			Expression<Func<T, bool>> predicate,
			int index,
			int take)
		{
			var q = PrimaryOrder(source)
				.Where(predicate)
				.Skip(index)
				.Take(take);
			return q;
		}

		#region --- ADD ---

		public virtual void Add(T entity)
		{
			var dbEntityEntry = _dbContext.Entry(entity);
			if(dbEntityEntry.State != EntityState.Detached)
				dbEntityEntry.State = EntityState.Added;
			else
				_dbSet.Add(entity);
		}

		#endregion

		#region --- UPDATE ---

		/// <summary>
		/// Adds the entity if the entity.Id is equal to default(T),
		/// else Updates instead.
		/// </summary>
		public virtual void Upsert(T entity)
		{
			if(entity == null)
				throw new ArgumentNullException();
			if(IdNotSet(entity)) // GetIdFromT(entity).Equals(_defaultId)) // entity.Id.Equals(_defaultId))
				Add(entity);
			else
				Update(entity);
		}

		/// <summary>
		/// Needed to fix: An object with the same key already exists in the ObjectStateManager.
		/// http://stackoverflow.com/a/12587752/264031
		/// </summary>
		/// <param name="entity"></param>
		public virtual void Update(T entity)
		{
			if(entity == null)
				throw new ArgumentException("Cannot add a null entity.");

			var entry = _dbContext.Entry<T>(entity);

			if(entry.State == EntityState.Detached) {
				T attachedEntity = _dbSet.Local.SingleOrDefault(e => MatchesId(e, entity)); // _getIdFromT(e).Equals(_getIdFromT(entity))); //e.Id.Equals(entity.Id));  // You need to have access to key
				if(attachedEntity != null) {
					var attachedEntry = _dbContext.Entry(attachedEntity);
					attachedEntry.CurrentValues.SetValues(entity);
				}
				else
					entry.State = EntityState.Modified; // This should attach entity
			}
		}

		/// <summary>
		/// 
		/// //http://stackoverflow.com/questions/5749110/readonly-properties-in-ef-4-1/5749469#5749469
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="properties"></param>
		public virtual void Update(T entity, params Expression<Func<T, object>>[] properties)
		{
			if(properties == null || properties.Length == 0)
				Update(entity);
			else {

				EntityEntry<T> entry = null;
				T attachedEntity = _dbSet.Local.SingleOrDefault(e => MatchesId(e, entity)); // _getIdFromT(e).Equals(_getIdFromT(entity))); // e.Id.Equals(entity.Id));  // You need to have access to key

				if(attachedEntity != null) {
					entry = _dbContext.Entry(attachedEntity);
					entry.CurrentValues.SetValues(entity);
				}

				if(entry == null) {
					entry = _dbContext.Entry(entity);
					_dbSet.Attach(entity);
				}

				foreach(var selector in properties)
					entry.Property(selector).IsModified = true;
			}
		}

		#endregion

		#region --- DELETE ---

		public virtual void Delete(T entity)
		{
			var dbEntityEntry = _dbContext.Entry(entity);
			if(dbEntityEntry != null && dbEntityEntry.State != EntityState.Deleted)
				dbEntityEntry.State = EntityState.Deleted;
			else {
				_dbSet.Attach(entity);
				_dbSet.Remove(entity);
			}
		}

		public virtual int Delete(TId id, bool? deleteDirect = null)
		{
			bool _deleteDirect = deleteDirect != null
				? (bool)deleteDirect
				: DeleteDirectDefault; // _dbSettings.DeleteDirectByDefault;
			if(_deleteDirect)
				return DeleteDirect(id);
			else {
				var entity = GetById(id);
				if(entity != null) // not found; assume already deleted.
					Delete(entity);
				return 0;
			}
		}

		#endregion

		#region --- Count ---

		public virtual int Count(Expression<Func<T, bool>> predicate = null)
		{
			if(predicate == null)
				return _dbSet.Count();
			return _dbSet.Count(predicate);
		}

		public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
		{
			if(predicate == null)
				return await _dbSet.CountAsync();
			return await _dbSet.CountAsync(predicate);
		}

		#endregion



		public int SaveChanges()
		{
			return _dbContext.SaveChanges();
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Call this before calling SaveAsync in order to turn on identity_insert,
		/// i.e. in order to save items to the database with identity columns already
		/// having a set value. For turn on, opens a database connection and then sends
		/// a sql command to turn identity insert on. For false, vise versa.
		/// </summary>
		/// <param name="turnOn">True to turn on, false to turn off.</param>
		public async Task<int> SetIdentityInsert(bool turnOn)
		{
			string command = $"SET IDENTITY_INSERT {FullTableName} {(turnOn ? "ON" : "OFF")}";

			if(turnOn)
				_db.OpenConnection();

			int result = await _db.ExecuteSqlRawAsync(command); // .ExecuteSqlCommandAsync(command); // .ExecuteSqlCommand(command);

			if(!turnOn)
				_db.CloseConnection();

			return result;
		}





		// --- DbExecuteSqlCommand ---

		protected int DbExecuteSqlCommand(string sql, params object[] args)
		{
			//int result = args.IsNulle()
			//	? _db.ExecuteSqlCommand(sql)
			//	: _db.ExecuteSqlCommand(sql, args);
			int result = args.IsNulle()
				? _db.ExecuteSqlRaw(sql)
				: _db.ExecuteSqlRaw(sql, args);
			return result;
		}

		protected async Task<int> DbExecuteSqlCommandAsync(string sql, params object[] args)
		{
			//int result = args.IsNulle()
			//	? await _db.ExecuteSqlCommandAsync(sql)
			//	: await _db.ExecuteSqlCommandAsync(sql, args); // TOTALLY stupid, if args is null, throws exception! but since it is a params, it should allow
			int result = args.IsNulle()
				? await _db.ExecuteSqlRawAsync(sql)
				: await _db.ExecuteSqlRawAsync(sql, args); // TOTALLY stupid, if args is null, throws exception! but since it is a params, it should allow
			return result;
		}

		public void Dispose()
		{
			_dbContext?.Dispose();
		}


		public TableMetaInfo GetNewTableInfo()
		{
			ITableMetaInfoBuilder tableMeta = _dbContext.GetTableMetaInfoBuilder();
			return tableMeta.GetTableMetaInfo(typeof(T));
		}

		public SQLQuery GetSQLQuery()
		{
			var q = new SQLQuery(TableMeta, PKOrderFinal);
			return q;
		}

		#region --- Was From DbRepositoryCoreBase ---

		#region --- DbExecuteSqlCommand ---

		protected int DbExecuteSqlCommand(string sql, SqlParam[] args = null)
		{
			SqlParameter[] _args = args?.Select(p => p.ToSqlParameter()).ToArray();
			return DbExecuteSqlCommand(sql, _args);
		}

		protected async Task<int> DbExecuteSqlCommandAsync(string sql, SqlParam[] args = null)
		{
			SqlParameter[] _args = args?.Select(p => p.ToSqlParameter()).ToArray();
			return await DbExecuteSqlCommandAsync(sql, _args);
		}

		#endregion

		#region --- UpdateDirect ---


		protected int __UpdateDirect(TId id, string whereClause, params SqlParam[] args)
		{
			var vals = __UpdateDirectHelper(id, whereClause, ref args);

			string sql = vals.Key;

			int result = DbExecuteSqlCommand(sql, args); // db.ExecuteSqlCommand(sql, _args);
			return result;
		}


		protected async Task<int> __UpdateDirectAsync(TId id, string whereClause, params SqlParam[] args)
		{
			var vals = __UpdateDirectHelper(id, whereClause, ref args);

			string sql = vals.Key;
			SqlParameter[] _args = vals.Value?.Select(p => p.ToSqlParameter()).ToArray();

			int result = await DbExecuteSqlCommandAsync(sql, _args);
			return result;
		}





		// ---------


		public virtual int UpdateDirect(TId id, params SqlParam[] updateColumns)
		{
			int result = __UpdateDirect(id, null, updateColumns);
			return result;
		}


		public virtual int UpdateDirect(TId id, string updateColumnName, object value)
		{
			return __UpdateDirect(id, null, new SqlParam(updateColumnName, value));
		}

		public virtual int UpdateDirect(params SqlParam[] updateColumns)
		{
			int result = __UpdateDirect(_defaultId, null, updateColumns);
			return result;
		}

		public virtual int UpdateDirect(string whereClause, params SqlParam[] updateColumns)
		{
			if(whereClause.IsNulle()) throw new ArgumentNullException();
			// note: we allow them to send in with no where filter, but do not allow
			// if they called this clause that exects it, for safety's sake
			int result = __UpdateDirect(_defaultId, whereClause, updateColumns);
			return result;
		}

		// ---------

		public async virtual Task<int> UpdateDirectAsync(TId id, string updateColumnName, object value)
		{
			return await __UpdateDirectAsync(id, null, new SqlParam(updateColumnName, value));
		}

		public async virtual Task<int> UpdateDirectAsync(TId id, params SqlParam[] updateColumns)
		{
			int result = await __UpdateDirectAsync(id, null, updateColumns);
			return result;
		}

		public async virtual Task<int> UpdateDirectAsync(params SqlParam[] updateColumns)
		{
			int result = await __UpdateDirectAsync(_defaultId, null, updateColumns);
			return result;
		}

		public async virtual Task<int> UpdateDirectAsync(string whereClause, params SqlParam[] updateColumns)
		{
			if(whereClause.IsNulle()) throw new ArgumentNullException();
			// note: we allow them to send in with no where filter, but do not allow
			// if they called this clause that exects it, for safety's sake
			int result = await __UpdateDirectAsync(_defaultId, whereClause, updateColumns);
			return result;
		}

		// ---------

		public async virtual Task<int> UpdateDirectAsync(TId id, Func<TTableDef, string> exp, object value)
		{
			string key = exp(_t);
			int result = await __UpdateDirectAsync(id, null, new SqlParam(key, value));
			return result;
		}

		public async virtual Task<int> UpdateDirectAsync(TId id, Func<TTableDef, KeyValuePair<string, object>> exp)
		{
			var ko = exp(_t);
			int result = await __UpdateDirectAsync(id, null, new SqlParam(ko.Key, ko.Value));
			return result;
		}

		public async virtual Task<int> UpdateDirectAsync(TId id, params KeyValuePair<string, object>[] vals)
		{
			return await __UpdateDirectAsync(id, null, vals.Select(k => new SqlParam(k.Key, k.Value)).ToArray());
		}

		public async virtual Task<int> UpdateDirectAsync(TId id, params KV[] vals)
		{
			return await __UpdateDirectAsync(id, null, vals.Select(k => new SqlParam(k.Key.ToString(), k.Value)).ToArray());
		}

		public async virtual Task<int> UpdateDirectAsync(TId id, Func<TTableDef, IEnumerable<KeyValuePair<string, object>>> exp)
		{
			return await __UpdateDirectAsync(id, null, exp(_t).Select(k => new SqlParam(k.Key, k.Value)).ToArray());
		}

		// ---------

		public virtual int UpdateDirect(TId id, Func<TTableDef, string> exp, object value)
		{
			string key = exp(_t);
			int result = __UpdateDirect(id, null, new SqlParam(key, value));
			return result;
		}

		public virtual int UpdateDirect(TId id, Func<TTableDef, KeyValuePair<string, object>> exp)
		{
			var ko = exp(_t);
			int result = __UpdateDirect(id, null, new SqlParam(ko.Key, ko.Value));
			return result;
		}

		public virtual int UpdateDirect(TId id, params KeyValuePair<string, object>[] vals)
		{
			return __UpdateDirect(id, null, vals.Select(k => new SqlParam(k.Key, k.Value)).ToArray());
		}

		public virtual int UpdateDirect(TId id, params KV[] vals)
		{
			return __UpdateDirect(id, null, vals.Select(k => new SqlParam(k.Key.ToString(), k.Value)).ToArray());
		}

		public virtual int UpdateDirect(TId id, Func<TTableDef, IEnumerable<KeyValuePair<string, object>>> exp)
		{
			return __UpdateDirect(id, null, exp(_t).Select(k => new SqlParam(k.Key, k.Value)).ToArray());
		}


		#endregion

		#region --- ExecuteProc ---

		public int ExecuteProc(string procName, TId id, params SqlParam[] args)
		{
			return ExecuteProc(procName, _CombineIdWithArgs(id, args));
		}

		public int ExecuteProc(string procName, TId id, params KeyValuePair<string, object>[] args)
		{
			return ExecuteProc(procName, _CombineIdWithArgs(id, args));
		}

		public int ExecuteProc(string procName, params SqlParam[] args)
		{
			string sql = _GetParametersString(procName, args);
			SqlParameter[] _args = args?.Select(p => p.ToSqlParameter()).ToArray();
			int result = DbExecuteSqlCommand(sql, _args);
			return result;
		}

		// ----------

		public async Task<int> ExecuteProcAsync(string procName, TId id, params SqlParam[] args)
		{
			return await ExecuteProcAsync(procName, _CombineIdWithArgs(id, args));
		}

		public async Task<int> ExecuteProcAsync(string procName, TId id, params KeyValuePair<string, object>[] args)
		{
			return await ExecuteProcAsync(procName, _CombineIdWithArgs(id, args));
		}

		public async Task<int> ExecuteProcAsync(string procName, params SqlParam[] args)
		{
			string sql = _GetParametersString(procName, args);
			SqlParameter[] _args = args?.Select(p => p.ToSqlParameter()).ToArray();
			int result = await DbExecuteSqlCommandAsync(sql, _args);
			return result;
		}

		#endregion

		#region --- DeleteDirect ---

		public virtual int DeleteDirect(TId id)
		{
			return DbExecuteSqlCommand(_DeleteDirectStr(id));
		}

		public virtual async Task<int> DeleteDirectAsync(TId id)
		{
			return await DbExecuteSqlCommandAsync(_DeleteDirectStr(id));
		}

		public virtual int DeleteAll()
		{
			string sql = $"DELETE {FullTableName}";
			return DbExecuteSqlCommand(sql);
		}

		public virtual int DeleteDirectWhere(string colName, object value, string operatr = "=")
		{
			return DbExecuteSqlCommand(_DeleteDirectStrWhere(colName, value, operatr));
		}

		public virtual async Task<int> DeleteDirectWhereAsync(string colName, object value, string operatr = "=")
		{
			return await DbExecuteSqlCommandAsync(_DeleteDirectStrWhere(colName, value, operatr));
		}

		string _DeleteDirectStr(TId id)
		{
			string sql = $"DELETE {FullTableName} WHERE {IdName} = {IdToString(id)}";
			return sql;
		}

		string _DeleteDirectStrWhere(string colName, object value, string operatr)
		{
			string val = value?.ToString();
			if(colName.IsNulle() || operatr.IsNulle() || val.IsNulle())
				throw new ArgumentNullException();
			string sql = $"DELETE {FullTableName} WHERE {colName} {operatr} {val}";
			return sql;
		}

		string _DeleteDirectStr(TId id, NameValueMatch nvm)
		{
			// whoa: this needs to change!!! use parameters...
			string sql = $"DELETE {FullTableName} WHERE {IdName} = {IdToString(id)} AND {nvm}";
			return sql;
		}

		public int DeleteDirect(TId id, NameValueMatch nvm)
		{
			// change to params!
			string sql = _DeleteDirectStr(id, nvm);
			return DbExecuteSqlCommand(sql);
		}

		public async Task<int> DeleteDirectAsync(TId id, NameValueMatch nvm)
		{
			// change to params!
			string sql = _DeleteDirectStr(id, nvm);
			return await DbExecuteSqlCommandAsync(sql);
		}

		public int DeleteDirect(NameValueMatch nvm)
		{
			string sql = $"DELETE {FullTableName} WHERE {nvm}";
			return DbExecuteSqlCommand(sql);
		}


		#endregion

		#region --- HELPERS ---

		protected KeyValuePair<string, SqlParam[]> __UpdateDirectHelper(TId id, string whereClause, ref SqlParam[] args)
		{
			if(args != null)
				args = args.Where(a => a != null).ToArray();
			if(args.IsNulle())
				throw new ArgumentNullException(); // huh? why was this required to always have args?

			string columnsSets = _GetParametersString(true, args);

			bool hasId = id != null && !id.Equals(_defaultId);
			if(hasId) {
				if(whereClause.NotNulle())
					throw new ArgumentException("Id and WHERE clause cannot both be set.");

				whereClause = $"WHERE {IdName} = @{IdName}";

				args = args.JoinSequences(new SqlParam(IdName, IdToString(id)));
			}

			string sql = $@"UPDATE {FullTableName}
SET {columnsSets}
{whereClause}".Trim();

			return new KeyValuePair<string, SqlParam[]>(sql, args);
		}


		// --- _CombineIdWithArgs ---

		protected SqlParam[] _CombineIdWithArgs(TId id, params KeyValuePair<string, object>[] args)
		{
			return _CombineIdWithArgs(id, args?.Select(a => new SqlParam(a.Key, a.Value)).ToArray());
		}

		protected SqlParam[] _CombineIdWithArgs(TId id, params SqlParam[] args)
		{
			if(id == null)
				return args;

			var idParam = new SqlParam(IdName, IdToString(id));
			if(args == null) {
				args = new SqlParam[1];
				args[0] = idParam;
			}
			else {
				var items = new List<SqlParam>(args.Length + 1);
				items.Add(idParam);
				items.AddRange(args);
				args = items.ToArray();
			}
			return args;
		}


		// --- _GetParametersString ---

		protected string _GetParametersString(bool updateSet_notProcCall, params SqlParam[] args)
		{
			return _GetParametersString(updateSet_notProcCall, null, args);
		}

		protected string _GetParametersString(string procName, params SqlParam[] args)
		{
			return _GetParametersString(false, procName, args);
		}

		protected string _GetParametersString(bool updateSet_notProcCall, string procName, params SqlParam[] args)
		{
			var sb = new StringBuilder(120);
			if(procName != null)
				sb.Append("EXEC ")
				  .Append(procName);

			if(args.NotNulle()) {
				for(int i = 0; i < args.Length; i++) {
					var p = args[i];
					if(p != null) {
						if(p.Value == null)
							p.Value = DBNull.Value;
						if(updateSet_notProcCall)
							sb.AppendFormat(" [{0}] = @{0},", args[i].Name); //.ParameterName);
						else
							sb.Append(" @").Append(args[i].Name).Append(",");
					}
				}
				if(sb[sb.Length - 1] == ',')
					sb.Length -= 1;
			}
			if(sb.Length > 0 && sb[0] == ' ')
				return sb.ToString(1, sb.Length - 1);
			return sb.ToString();
		}



		protected void __PrepQueryIfNeeded(SQLQuery query)
		{
			if(query.TableNameFull == null)
				query.TableNameFull = this.FullTableName;
		}

		#endregion

		#endregion

		public string GetTableDefinitionsCode(
			bool allDbSets,
			TDCodeGenerator codeGenOps = null,
			bool withNamespace = true)
		{
			ITableMetaInfoBuilder tmiBldr = _dbContext.GetTableMetaInfoBuilder();

			string code = TDCodeGeneratorX.GetTableDefinitionsCodeForEntityTypes(
				tmiBldr,
				tmiBldr.GetEntityTypes(),
				codeGenOps,
				withNamespace);

			return code;
		}

	}
}
