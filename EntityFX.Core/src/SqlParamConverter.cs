using System;
using System.Data;
using System.Linq;

using Microsoft.Data.SqlClient;

namespace EntityFX.Core
{
	public static class SystemDataEFXExtensions
	{
		public static SqlParameter ToSqlParameter(this SqlParam sqlParam)
		{
			if(sqlParam == null) return null;
			SqlParameter p = new SqlParameter(sqlParam.Name, sqlParam.Value ?? DBNull.Value);
			if(sqlParam.SqlType != null)
				p.SqlDbType = sqlParam.SqlType.Value.ToSqlDbType();
			if(sqlParam.Direction != null)
				p.Direction = sqlParam.Direction.Value.ToParameterDirection();
			return p;
		}

		public static string ToSQL(this SQLQuery q, out SqlParameter[] args)
		{
			string sql = q.ToSQL(out SqlParam[] _args);

			args = _args?.Select(a => a.ToSqlParameter()).ToArray();
			return sql;
		}

		public static SqlDbType ToSqlDbType(this SqlDbTyp typ)
		{
			return (SqlDbType)(int)typ;
		}

		public static ParameterDirection ToParameterDirection(this ParamDirection dir)
		{
			return (ParameterDirection)(int)dir;
		}

	}
}
