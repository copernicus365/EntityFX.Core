using System.ComponentModel.DataAnnotations.Schema;

using EntityFX.Core;

namespace Test.EntityFX.Core.Mock
{
	[Table(TableName)]
	[PKOrder(PKOrder)]
	public class WidgetEnt
	{
		public const string TableName = "Widgy";
		public const string PKOrder = "GroupId ASC, Id DESC";

		public int GroupId { get; set; }

		public int Id { get; set; }

		[Column("XName")]
		public string Name { get; set; }
	}
}
