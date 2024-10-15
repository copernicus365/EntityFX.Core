using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public interface IDbRepositoryBasic<T> : IDbRepositoryBasic<T, int> where T : class { }

public interface IDbRepositoryBasic<T, TId> where T : class
{
	// --- PROPERTIES ---

	bool AsNoTracking { get; set; }
	Func<IQueryable<T>, IOrderedQueryable<T>> GetPrimaryOrder { get; set; }


	// --- ABSTRACT MEMBERS ---

	bool IdNotSet(T item);
	bool MatchesId(T item1, T item2);
	IQueryable<T> WhereMatchTId(IQueryable<T> source, TId id);


	// --- GET ---

	IQueryable<T> Get(bool? noTracking = null);
	T GetById(TId id, bool? noTracking = null);
	Task<T> GetByIdAsync(TId id, bool? noTracking = null);


	IQueryable<T> GetRange(Expression<Func<T, bool>> predicate, int index, int take, bool? noTracking = null);
	IQueryable<T> GetRange(int index, int take, bool? noTracking = null);
	IQueryable<T> GetRange(IQueryable<T> source, Expression<Func<T, bool>> predicate, int index, int take);
	IQueryable<T> GetRange(IQueryable<T> source, int index, int take);


	int Count(Expression<Func<T, bool>> predicate = null);
	Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);


	void Add(T entity);
	void Update(T entity);
	void Update(T entity, params Expression<Func<T, object>>[] properties);
	void Upsert(T entity);
	void Delete(T entity);
	int Delete(TId id, bool? deleteDirect = null);


	int SaveChanges();
	Task<int> SaveChangesAsync();
}
