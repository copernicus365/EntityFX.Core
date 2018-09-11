using System.Diagnostics;

namespace EntityFX.Core
{
	/// <summary>
	/// Represents a name-value match pair as well as the match operator, 
	/// useful for instance for SQL WHERE clause construction or representation.
	/// </summary>
	public struct NameValueMatch
	{
		public string Name;
		public object Value;
		public string Operatr;

		[DebuggerStepThrough]
		public NameValueMatch(string name, object value, string operatr = "=")
		{
			Name = name;
			Value = value;
			Operatr = operatr;
		}

		public override string ToString()
		{
			return Name + " " + Operatr + " " + Value;
		}
	}
}
