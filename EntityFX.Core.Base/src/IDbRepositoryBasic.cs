using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public interface IDbRepositoryBasic<T> : IDbRepositoryBasic<T, int> where T : class { }

public interface IDbRepositoryBasic<T, TId> where T : class
{
	// --- PROPERTIES ---

	bool AsNoTracking { get; set; }
	Func<IQueryable<T>, IOrderedQueryable<T>> PrimaryOrder { get; set; }

	// --- ABSTRACT MEMBERS ---

	string IdToString(TId id);

	bool IdNotSet(T item);
	bool MatchesId(T item1, T item2);
	IQueryable<T> WhereMatchTId(IQueryable<T> source, TId id);


	int Count(Expression<Func<T, bool>> predicate = null);
	Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);


	// --- CUD ---

	void Add(T entity);
	void Update(T entity);
	void Update(T entity, params Expression<Func<T, object>>[] properties);
	void Upsert(T entity);

	void Delete(T entity);
	int Delete(TId id);
	int DeleteDirect(TId id);
	Task<int> DeleteDirectAsync(TId id);


	// --- GET ---

	T GetById(TId id, bool? noTracking = null);
	Task<T> GetByIdAsync(TId id, bool? noTracking = null);

	IQueryable<T> Get(bool? noTracking = null);

	IQueryable<T> GetRange(int index, int take, Expression<Func<T, bool>> predicate = null, bool? noTracking = null);
	IQueryable<T> GetRange(IQueryable<T> source, int index, int take, Expression<Func<T, bool>> predicate = null);

	IQueryable<T> GET_PrimaryOrderedOrDefault(bool? noTracking = null);
	IQueryable<T> GET_PrimaryOrderedOrDefault(IQueryable<T> source);


	int SaveChanges();
	Task<int> SaveChangesAsync();
}
