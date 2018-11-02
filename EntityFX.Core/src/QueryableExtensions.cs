using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EntityFX.QueryableExtensions
{
	/// <summary>
	/// Both the old Entity Framework and the new Entity Framework Core expose async
	/// extension methods on IQueryable, such as: `ToArrayAsync`, `FirstOrDefaultAsync`, 
	/// and so forth. The old way required a using statement as follows: 
	/// `using System.Data.Entity;`, whereas
	/// the new way simply uses the same base namespace for the rest of EFCore to be set:
	/// `using Microsoft.EntityFrameworkCore;`. The problem arises, however, when a single
	/// code file has IQueryable methods exposed (typically through repository interfaces)
	/// which have a backing source from both of these, the old and the new EF. Doing this causes
	/// exceptions. For instance, calling `ToArrayAsync` on an IQueryable whose backing store
	/// was the new EFCore, will throw an exception if the `ToArrayAsync` actually came from your
	/// `System.Data.Entity;` using statement.
	/// <para />
	/// What this class allows you to do is to get around this problem, by keeping the 
	/// `using System.Data.Entity;` reference, but then by adding a using statement 
	/// to this class, namely: `using EntityFX.QueryableExtensions;` You still have to know
	/// which queryables came from EFCore, but then you can call one of the methods herein,
	/// such as `ToArrayEFCoreAsync`, and so forth. Since the name is different, there is no 
	/// conflict. You still have to be careful to use only these methods on IQueryables 
	/// that came from EFCore, but this way, everyone plays happily in the same playground.
	/// </summary>
	public static class EFQueryableExtensions
	{
		public static Task<TSource> FirstOrDefaultEFCoreAsync<TSource>(
			this IQueryable<TSource> source, 
			Expression<Func<TSource, bool>> predicate, 
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
				source, predicate, cancellationToken);
		}

		public static Task<TSource[]> ToArrayEFCoreAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return EntityFrameworkQueryableExtensions.ToArrayAsync(
				source, cancellationToken);
		}

		public static Task<List<TSource>> ToListEFCoreAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return EntityFrameworkQueryableExtensions.ToListAsync(
				source, cancellationToken);
		}

	}
}
