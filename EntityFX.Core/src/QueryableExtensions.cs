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
	/// The purpose of this is for cases where client code (that is just 
	/// working with IQueryable, NOT even anything else) access IQueryable
	/// sources sometimes where the source is from the old EntityFramework, 
	/// and other cases where it is Entity Framework Core. The old way of getting
	/// the `ToArrayAsync` etc methods required the namespace System.Data, whereas
	/// the new EFCore simply requires the `Microsoft.EntityFrameworkCore` namespace.
	/// So to reference each of these causes a conflict if not calling from the right
	/// one. This class therefore provides intermediates to those extension methods
	/// for EFCore, and it's important to note that this requires a special namespace
	/// different from the rest of this project: <see cref="EntityFX.QueryableExtensions"/>.
	/// Set that as a using only when there is going to be a possible mixture between
	/// old and new. Doing so then will make these explicitly for EFCore extension 
	/// methods available.
	/// </summary>
	public static class EFQueryableExtensions
	{
		public static Task<TSource> FirstOrDefaultAsync_EFCore<TSource>(
			this IQueryable<TSource> source, 
			Expression<Func<TSource, bool>> predicate, 
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
				source, predicate, cancellationToken);
		}

		public static Task<TSource[]> ToArrayAsync_EFCore<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return EntityFrameworkQueryableExtensions.ToArrayAsync(
				source, cancellationToken);
		}

		public static Task<List<TSource>> ToListAsync_EFCore<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return EntityFrameworkQueryableExtensions.ToListAsync(
				source, cancellationToken);
		}

	}
}
