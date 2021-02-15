using System.Diagnostics;

namespace EntityFX.Core
{
	/// <summary>
	/// A simple object based KeyValue pair type.
	/// </summary>
	public class KV
	{
		/// <summary>
		/// KV Key.
		/// </summary>
		public object Key { get; set; }

		/// <summary>
		/// KV Value.
		/// </summary>
		public object Value { get; set; }

		[DebuggerStepThrough]
		public KV() { }

		[DebuggerStepThrough]
		public KV(object key, object value)
		{
			Key = key;
			Value = value;
		}

		public override string ToString()
			=> $"[{Key}, {Value}]";
	}
}
