using System.Linq;

using EntityFX.Core;

using Microsoft.EntityFrameworkCore;

namespace Test.EntityFX.Core.Mock
{
	public class WidgetRepo
		: DbRepositoryCore<WidgetEnt, int>, IWidgetRepo
	{
		public WidgetRepo(DbContext dbContext)
			: base(dbContext)
		{
		}

		public override IQueryable<WidgetEnt> WhereMatchTId(IQueryable<WidgetEnt> source, int id)
			=> source.Where(itm => itm.Id == id);

		public override bool IdNotSet(WidgetEnt item)
			=> item.Id == 0;

		public override bool MatchesId(WidgetEnt item1, WidgetEnt item2)
			=> item1.Id == item2.Id;

		public override IOrderedQueryable<WidgetEnt> PrimaryOrder(IQueryable<WidgetEnt> source)
			=> source
			.OrderBy(e => e.GroupId)
			.ThenByDescending(e => e.Id);

	}

	public interface IWidgetRepo
		: IDbRepositoryCore<WidgetEnt, int>
	{
	}
}
