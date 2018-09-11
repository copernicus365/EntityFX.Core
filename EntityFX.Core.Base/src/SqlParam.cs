using System.Diagnostics;

namespace EntityFX.Core
{
	public class SqlParam
	{
		[DebuggerStepThrough]
		public SqlParam() { }

		[DebuggerStepThrough]
		public SqlParam(string name, object value)
		{
			Name = name;
			Value = value;
		}

		[DebuggerStepThrough]
		public SqlParam(string name, object value, SqlDbTyp sqlDbType)
		{
			Name = name;
			Value = value;
			SqlType = sqlDbType;
		}

		public string Name { get; set; }

		public object Value { get; set; }

		public SqlDbTyp? SqlType { get; set; }

		public ParamDirection? Direction { get; set; }
	}
}
