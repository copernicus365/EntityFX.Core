using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFX.Core;

public abstract class DbRepositoryBasic<T> : DbRepositoryBasic<T, int> where T : class
{
	public DbRepositoryBasic(DbContext context) : base(context) { }
}

public abstract class DbRepositoryBasic<T, TId> : IDbRepositoryBasic<T, TId> where T : class
{
	// --- FIELDS ---

	protected DbContext _dbContext;
	protected DbSet<T> _dbSet;
	protected DatabaseFacade _db;

	// === STATIC FIELDS ===

	public static string FullTableName { get; private set; }
	public static string IdName { get; set; } = "Id";


	public DbRepositoryBasic(DbContext dbContext)
	{
		ArgumentNullException.ThrowIfNull(dbContext);

		_dbContext = dbContext;
		_dbSet = _dbContext.Set<T>();
		_db = _dbContext.Database;

		if(FullTableName == null) {
			FullTableName = EFCoreXtensions.GetTableSchemaName<T>(_dbContext);
		}
	}

	#region --- abstract / virtual members  ---

	public virtual string IdToString(TId id) => id?.ToString();

	public abstract IQueryable<T> WhereMatchTId(IQueryable<T> source, TId id);

	public abstract bool IdNotSet(T item);

	public abstract bool MatchesId(T item1, T item2);


	public bool AsNoTracking { get; set; } = true;

	/// <summary>
	/// Set to true in order to disable Upsert. Upsert must call
	/// <see cref="IdNotSet"/>, but that is only useful on Identity types
	/// (like with an integer Id), where at ADD time it's value is default.
	/// While e.g. composite primary keys will always need to be set for both
	/// Add and Update, in which case Upsert can't be called.
	/// </summary>
	public virtual bool DisableUpsert { get; }

	public Func<IQueryable<T>, IOrderedQueryable<T>> PrimaryOrder { get; set; }

	#endregion


	#region --- GET ---

	public T GetById(TId id, bool? noTracking = null)
		=> WhereMatchTId(Get(noTracking ?? AsNoTracking), id).FirstOrDefault();

	public async Task<T> GetByIdAsync(TId id, bool? noTracking = null)
		=> await WhereMatchTId(Get(noTracking ?? AsNoTracking), id).FirstOrDefaultAsync();

	public IQueryable<T> Get(bool? noTracking = null)
		=> noTracking ?? AsNoTracking ? _dbSet.AsNoTracking() : _dbSet;

	/// <summary>
	/// If <see cref="PrimaryOrder"/> is set, will pass the input <paramref name="source"/>
	/// through that to have it's items returned in that ordered way. ELSE just returns
	/// input source with no problems.
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public IQueryable<T> GET_PrimaryOrderedOrDefault(IQueryable<T> source)
	{
		ArgumentNullException.ThrowIfNull(source);

		return PrimaryOrder == null
			? source
			: PrimaryOrder(source);
	}

	public IQueryable<T> GET_PrimaryOrderedOrDefault(bool? noTracking = null)
	{
		var q = Get(noTracking);
		return PrimaryOrder == null
			? q
			: PrimaryOrder(q);
	}

	#endregion

	#region --- GetRange ---

	public IQueryable<T> GetRange(
		int index,
		int take,
		Expression<Func<T, bool>> predicate = null,
		bool? noTracking = null)
	{
		var q = EFCoreXtensions.WhereIf(GET_PrimaryOrderedOrDefault(noTracking), predicate != null, predicate)
			.Skip(index)
			.Take(take);
		return q;
	}

	public IQueryable<T> GetRange(
		IQueryable<T> source,
		int index,
		int take,
		Expression<Func<T, bool>> predicate = null)
	{
		var q = EFCoreXtensions.WhereIf(source, predicate != null, predicate)
			.Skip(index)
			.Take(take);
		return q;
	}

	#endregion


	#region --- Count ---

	public int Count(Expression<Func<T, bool>> predicate = null)
		=> predicate == null ? _dbSet.Count() : _dbSet.Count(predicate);

	public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
		=> predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

	#endregion


	// --- CUD ---

	public void Add(T entity)
	{
		var dbEntityEntry = _dbContext.Entry(entity);
		if(dbEntityEntry.State != EntityState.Detached)
			dbEntityEntry.State = EntityState.Added;
		else
			_dbSet.Add(entity);
	}


	#region --- UPDATE ---

	/// <summary>
	/// Adds the entity if the entity.Id is equal to default(T),
	/// else Updates instead.
	/// </summary>
	public void Upsert(T entity)
	{
		ArgumentNullException.ThrowIfNull(entity);
		if(DisableUpsert) throw new Exception("UPSERT is disabled for this type, see `DisableUpsert` property");

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
	public void Update(T entity)
	{
		ArgumentNullException.ThrowIfNull(entity);

		var entry = _dbContext.Entry(entity);

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
	public void Update(T entity, params Expression<Func<T, object>>[] properties)
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

	public void Delete(T entity)
	{
		var dbEntityEntry = _dbContext.Entry(entity);
		if(dbEntityEntry != null && dbEntityEntry.State != EntityState.Deleted)
			dbEntityEntry.State = EntityState.Deleted;
		else {
			_dbSet.Attach(entity);
			_dbSet.Remove(entity);
		}
	}

	public int Delete(TId id)
	{
		var entity = GetById(id);
		if(entity != null) // not found; assume already deleted.
			Delete(entity);
		return 0;
	}


	// --- DeleteDirect ---

	public virtual string GetDeleteDirectSQL(TId id)
	{
		string sql = $"DELETE {FullTableName} WHERE {IdName} = {IdToString(id)}";
		return sql;
	}

	public int DeleteDirect(TId id)
		=> DbExecuteSqlCommand(GetDeleteDirectSQL(id));

	public async Task<int> DeleteDirectAsync(TId id)
	{
		string sql = GetDeleteDirectSQL(id);
		return await DbExecuteSqlCommandAsync(sql);
	}

	#endregion


	public int SaveChanges()
		=> _dbContext.SaveChanges();

	public async Task<int> SaveChangesAsync()
		=> await _dbContext.SaveChangesAsync();




	// --- DbExecuteSqlCommand ---

	protected int DbExecuteSqlCommand(string sql, params object[] args)
	{
		int result = args == null || args.Length < 1
			? _db.ExecuteSqlRaw(sql)
			: _db.ExecuteSqlRaw(sql, args);
		return result;
	}

	protected async Task<int> DbExecuteSqlCommandAsync(string sql, params object[] args)
	{
		int result = args == null || args.Length < 1
			? await _db.ExecuteSqlRawAsync(sql)
			: await _db.ExecuteSqlRawAsync(sql, args); // TOTALLY stupid, if args is null, throws exception! but since it is a params, it should allow
		return result;
	}

	//public void Dispose() => _dbContext?.Dispose();
}

internal static class EFCoreXtensions
{
	/// <summary>
	/// Gets the Schema Table name for the DbSet.
	/// Source: https://stackoverflow.com/a/69898129/264031
	/// </summary>
	internal static string GetTableSchemaName<T>(this DbContext context) where T : class
	{
		//DbContext context = dbSet.GetService<ICurrentDbContext>().Context;
		System.Type entityType = typeof(T);
		IEntityType m = context.Model.FindEntityType(entityType);
		return m.GetSchemaQualifiedTableName();
	}

	internal static IQueryable<T> WhereIf<T>(IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
	{
		if(condition)
			return source.Where(predicate);
		return source;
	}

	/// <summary>Gets the Schema Table name for the DbSet.</summary>
	internal static string GetTableSchemaName<T>(this DbSet<T> dbSet) where T : class
		=> GetTableSchemaName<T>(dbSet.GetService<ICurrentDbContext>().Context);
}
