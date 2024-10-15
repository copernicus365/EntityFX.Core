using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

	protected static TId _defaultId = default;


	public DbRepositoryBasic(DbContext dbContext)
	{
		ArgumentNullException.ThrowIfNull(_dbContext);

		_dbContext = dbContext;
		_dbSet = _dbContext.Set<T>();
		_db = _dbContext.Database;
		//context.Configuration.ValidateOnSaveEnabled = settings.ValidateOnSaveEnabled;
	}

	#region --- Abstract members (and any virtual members as prime candidates to consider implementing)  ---

	public abstract IQueryable<T> WhereMatchTId(IQueryable<T> source, TId id);

	public abstract bool IdNotSet(T item);

	public abstract bool MatchesId(T item1, T item2);

	public bool AsNoTracking { get; set; } = true;

	public Func<IQueryable<T>, IOrderedQueryable<T>> GetPrimaryOrder { get; set; }

	#endregion


	#region --- GET ---

	public virtual T GetById(TId id, bool? noTracking = null)
		=> WhereMatchTId(Get(noTracking ?? AsNoTracking), id).FirstOrDefault();

	public virtual async Task<T> GetByIdAsync(TId id, bool? noTracking = null)
		=> await WhereMatchTId(Get(noTracking ?? AsNoTracking), id).FirstOrDefaultAsync();

	public virtual IQueryable<T> Get(bool? noTracking = null)
		=> noTracking ?? AsNoTracking ? _dbSet.AsNoTracking() : _dbSet;

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
		var q = (GetPrimaryOrder == null ? source : GetPrimaryOrder(source))
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
		var q = (GetPrimaryOrder == null ? source : GetPrimaryOrder(source))
			.Where(predicate)
			.Skip(index)
			.Take(take);
		return q;
	}



	// --- ADD ---

	public virtual void Add(T entity)
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
	public virtual void Upsert(T entity)
	{
		ArgumentNullException.ThrowIfNull(entity);
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
		ArgumentNullException.ThrowIfNull(entity);

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
		var entity = GetById(id);
		if(entity != null) // not found; assume already deleted.
			Delete(entity);
		return 0;
	}

	#endregion



	// --- Count ---

	public virtual int Count(Expression<Func<T, bool>> predicate = null)
		=> predicate == null ? _dbSet.Count() : _dbSet.Count(predicate);

	public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
		=> predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);



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

	public void Dispose()
		=> _dbContext?.Dispose();

}
