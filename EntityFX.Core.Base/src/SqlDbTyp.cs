﻿
namespace EntityFX.Core
{
	/// <summary>
	/// Specifies the SQL Server data type. 
	/// Partially adapted from: https://github.com/dotnet/corefx/blob/master/src/System.Data.SqlClient/src/System/Data/SqlDbType.cs 
	/// Copy of the System.Data.SqlDbType enum type, so we can work without System.Data assembly.
	/// Original description: "Specifies SQL Server-specific data type of a field, property, 
	/// for use in a System.Data.SqlClient.SqlParameter." In our use, replace "System.Data.SqlClient.SqlParameter"
	/// with EntityFX.SqlParam.
	/// 
	/// (Originally contained at: 
	///  Assembly System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
	///  C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\System.Data.dll)
	/// </summary>
	public enum SqlDbTyp
	{
		/// <summary>System.Int64. A 64-bit signed integer.</summary>
		BigInt = 0,

		/// <summary>System.Array of type System.Byte. A fixed-length stream of binary data ranging between 1 and 8,000 bytes.</summary>
		Binary = 1,

		/// <summary>System.Boolean. An unsigned numeric value that can be 0, 1, or null.</summary>
		Bit = 2,

		/// <summary>System.String. A fixed-length stream of non-Unicode characters ranging between 1 and 8,000 characters.</summary>
		Char = 3,

		/// <summary>System.DateTime. Date and time data ranging in value from January 1, 1753 to December 31, 9999 to an accuracy of 3.33 milliseconds.</summary>
		DateTime = 4,

		/// <summary>System.Decimal. A fixed precision and scale numeric value between -10 38 -1 and 10 38 -1.</summary>
		Decimal = 5,

		/// <summary>System.Double. A floating point number within the range of -1.79E +308 through 1.79E +308.</summary>
		Float = 6,

		/// <summary>
		/// System.Array of type System.Byte. 
		/// A variable-length stream of binary data ranging from 0 to 2 31 -1 
		/// (or 2,147,483,647) bytes.
		/// </summary>
		Image = 7,

		/// <summary>System.Int32. A 32-bit signed integer.</summary>
		Int = 8,

		/// <summary>
		/// System.Decimal. A currency value ranging from -2 63 (or -9,223,372,036,854,775,808) 
		/// to 2 63 -1 (or +9,223,372,036,854,775,807) with an accuracy to a ten-thousandth of 
		/// a currency unit.
		/// </summary>
		Money = 9,

		/// <summary>System.String. A fixed-length stream of Unicode characters ranging between 1 and 4,000 characters.</summary>
		NChar = 10,

		/// <summary>System.String. A variable-length stream of Unicode data with a maximum length of 2 30 - 1 (or 1,073,741,823) characters.</summary>
		NText = 11,

		/// <summary>
		/// System.String. A variable-length stream of Unicode characters ranging 
		/// between 1 and 4,000 characters. Implicit conversion fails if the string is greater 
		/// than 4,000 characters. Explicitly set the object when working with strings longer 
		/// than 4,000 characters. Use System.Data.SqlDbType.NVarChar when the database column 
		/// is nvarchar(max).
		/// </summary>
		NVarChar = 12,

		/// <summary>System.Single. A floating point number within the range of -3.40E +38 through 3.40E +38.</summary>
		Real = 13,

		/// <summary>System.Guid. A globally unique identifier (or GUID).</summary>
		UniqueIdentifier = 14,

		/// <summary>System.DateTime. Date and time data ranging in value from January 1, 1900 to June 6, 2079 to an accuracy of one minute.</summary>
		SmallDateTime = 15,

		/// <summary>System.Int16. A 16-bit signed integer.</summary>
		SmallInt = 16,

		/// <summary>System.Decimal. A currency value ranging from -214,748.3648 to +214,748.3647 with an accuracy to a ten-thousandth of a currency unit.</summary>
		SmallMoney = 17,

		/// <summary>System.String. A variable-length stream of non-Unicode data with a maximum length of 2 31 -1 (or 2,147,483,647) characters.</summary>
		Text = 18,

		/// <summary>System.Array of type System.Byte. Automatically generated binary numbers, which are guaranteed to be unique within a database. timestamp is used typically as a mechanism for version-stamping table rows. The storage size is 8 bytes.</summary>
		Timestamp = 19,

		/// <summary>System.Byte. An 8-bit unsigned integer.</summary>
		TinyInt = 20,

		/// <summary>System.Array of type System.Byte. A variable-length stream of binary data ranging between 1 and 8,000 bytes. Implicit conversion fails if the byte array is greater than 8,000 bytes. Explicitly set the object when working with byte arrays larger than 8,000 bytes.</summary>
		VarBinary = 21,

		/// <summary>System.String. A variable-length stream of non-Unicode characters ranging between 1 and 8,000 characters. Use System.Data.SqlDbType.VarChar when the database column is varchar(max).</summary>
		VarChar = 22,

		/// <summary>System.Object. A special data type that can contain numeric, string, binary, or date data as well as the SQL Server values Empty and Null, which is assumed if no other type is declared.</summary>
		Variant = 23,

		/// <summary>An XML value. Obtain the XML as a string using the System.Data.SqlClient.SqlDataReader.GetValue(System.Int32) method or System.Data.SqlTypes.SqlXml.Value property, or as an System.Xml.XmlReader by calling the System.Data.SqlTypes.SqlXml.CreateReader method.</summary>
		Xml = 25,

		/// <summary>A SQL Server user-defined type (UDT).</summary>
		Udt = 29,

		/// <summary>A special data type for specifying structured data contained in table-valued parameters.</summary>
		Structured = 30,

		/// <summary>Date data ranging in value from January 1,1 AD through December 31, 9999 AD.</summary>
		Date = 31,

		/// <summary>Time data based on a 24-hour clock. Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds. Corresponds to a SQL Server time value.</summary>
		Time = 32,

		/// <summary>Date and time data. Date value range is from January 1,1 AD through December 31, 9999 AD. Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds.</summary>
		DateTime2 = 33,

		/// <summary>Date and time data with time zone awareness. Date value range is from January 1,1 AD through December 31, 9999 AD. Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds. Time zone value range is -14:00 through +14:00.</summary>
		DateTimeOffset = 34,
	}
}
