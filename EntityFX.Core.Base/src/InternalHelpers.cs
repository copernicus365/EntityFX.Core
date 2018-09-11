using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace EntityFX.Core
{
	/// <summary>
	/// Most of these are duplications of DotNetXtensions, copied here so 
	/// we do not need that external ref.
	/// </summary>
	internal static class InternalHelpers
	{ 
		internal static StringBuilder TrimEnd(this StringBuilder sb)
		{
			if (sb == null || sb.Length == 0) return sb;

			int i = sb.Length - 1;
			for (; i >= 0; i--)
				if (!char.IsWhiteSpace(sb[i]))
					break;

			if (i < sb.Length - 1)
				sb.Length = i + 1;

			return sb;
		}

		[DebuggerStepThrough]
		internal static bool TryGetValueAny<TKey, TVal>(this IDictionary<TKey, TVal> dict, out TVal val, params TKey[] values)
		{
			if (dict != null && dict.Count > 0 && values != null && values.Length > 0) {
				for (int i = 0; i < values.Length; i++)
					if (dict.TryGetValue(values[i], out val))
						return true;
			}
			val = default(TVal);
			return false;
		}

		#region IsNullOrEmpty | EmptyIfNull

		[DebuggerStepThrough]
		internal static bool IsNulle(this string str)
		{
			return str == null || str == "";
		}

		[DebuggerStepThrough]
		internal static bool NotNulle(this string str)
		{
			return str != null && str != "";
		}

		[DebuggerStepThrough]
		internal static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}

		/// <summary>
		/// Returns null if input is an empty or whitespace string, 
		/// else returns the (trimmed if needed) input.
		/// </summary>
		/// <param name="s">String</param>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static string NullIfEmptyTrimmed(this string s)
		{
			s = s.TrimIfNeeded();
			return s == "" ? null : s;
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static string TrimIfNeeded(this string s)
		{
			if (s.IsTrimmable())
				return s.Trim();
			return s;
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsTrimmable(this string s)
		{
			if (s != null) {
				int len = s.Length;
				if (len > 1)
					return char.IsWhiteSpace(s[0]) || char.IsWhiteSpace(s[len - 1]);
				return len == 0 || char.IsWhiteSpace(s[0]);
			}
			return false;
		}


		// ==========


		[DebuggerStepThrough]
		internal static bool IsNulle<TSource>(this TSource[] source)
		{
			return (source == null || source.Length == 0)
				? true
				: false;
		}

		[DebuggerStepThrough]
		internal static bool IsNulle<TSource>(this ICollection<TSource> source)
		{
			return (source == null || source.Count == 0)
				? true
				: false;
		}

		[DebuggerStepThrough]
		internal static bool NotNulle<TSource>(this TSource[] source)
		{
			return !(source == null || source.Length == 0)
				? true
				: false;
		}

		[DebuggerStepThrough]
		internal static bool NotNulle<TSource>(this ICollection<TSource> source)
		{
			return !(source == null || source.Count == 0)
				? true
				: false;
		}

		#endregion

		#region JoinToString

		// See XString for the string type equivalent (using the more efficient String.Join for that type)

		[DebuggerStepThrough]
		internal static string JoinToString<T>(this IEnumerable<T> thisEnumerable, string separator = ",")
		{
			if (thisEnumerable == null) throw new ArgumentNullException("thisEnumerable");
			if (separator == null) throw new ArgumentNullException("separator");

			StringBuilder sb = new StringBuilder("");

			foreach (T item in thisEnumerable)
				sb.Append(item.ToString() + separator);

			if (sb.Length == 0)
				return "";
			if (sb.Length > separator.Length)
				sb.Length = sb.Length - separator.Length; // get rid of the last separator append

			return sb.ToString();
		}

		[DebuggerStepThrough]
		internal static string JoinToString(this string[] theseStrings, string separator = ",")
		{
			return String.Join(separator, theseStrings);
		}

		#endregion

		#region JoinSequences

		internal static T[] JoinSequences<T>(this IList<T> seq1, IList<T> seq2)
		{
			if (seq1 == null && seq2 == null) throw new ArgumentNullException();
			if (seq1 == null)
				return seq2.ToArray();
			else if (seq2 == null)
				return seq1.ToArray();

			int seq1Cnt = seq1.Count;
			int seq2Cnt = seq2.Count;
			T[] combined = new T[seq1Cnt + seq2Cnt];

			int i = 0;
			for (int j = 0; j < seq1Cnt; j++, i++)
				combined[i] = seq1[j];

			for (int j = 0; j < seq2Cnt; j++, i++)
				combined[i] = seq2[j];

			return combined;
		}

		internal static T[] JoinSequences<T>(this IEnumerable<T> seq1, IEnumerable<T> seq2)
		{
			return JoinSequences<T>(
				seq1?.ToArray(),
				seq2?.ToArray());
		}

		internal static T[] JoinSequences<T>(this IEnumerable<T> seq1, T item)
		{
			return JoinSequences<T>(
				seq1?.ToArray(),
				item == null ? null : new T[] { item });
		}

		#endregion

		internal static T E<T>(this T t) where T : new()
		{
			if (t != null)
				return t;
			return new T();
		}

		internal static StringBuilder AppendMany(this StringBuilder sb, params object[] items)
		{
			if (sb == null || items == null || items.Length == 0)
				return sb;

			for (int i = 0; i < items.Length; i++)
				sb.Append(items[i]);

			return sb;
		}

		internal static string JoinToString(this IEnumerable<string> arr, string separator)
		{
			return String.Join(separator, arr);
		}

	}
}

namespace EntityFX.Core.Internal
{
	internal static class TableDefinitionsHelpers
	{
		/// <summary>
		/// From: http://stackoverflow.com/a/29379834/264031
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException("assembly");
			try {
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e) {
				return e.Types.Where(t => t != null);
			}
		}

		/// <summary>
		/// Gets types that implement this interface within the input Assembly.
		/// From: http://stackoverflow.com/a/29379834/264031
		/// </summary>
		/// <param name="interfaceType">Interface type, must be an interface.</param>
		/// <param name="assembly">Assembly</param>
		internal static IEnumerable<Type> GetTypesImplementingThisInterface(this Type interfaceType, Assembly assembly)
		{
			if (interfaceType == null || assembly == null)
				return null;

			if (!interfaceType.IsInterface)
				throw new ArgumentException("Input type must be the type of an interface.");

			return assembly.GetLoadableTypes().Where(interfaceType.IsAssignableFrom).ToList();
		}

		/// <summary>
		/// Gets types that implement this interface within the assembly in which the type 
		/// was declared. To get others implemented outside of the declaring assembly see the 
		/// overload where the Assembly can be passed in.
		/// </summary>
		/// <param name="interfaceType">Interface type, must be an interface.</param>
		internal static IEnumerable<Type> GetTypesImplementingThisInterface(this Type interfaceType)
		{
			if (interfaceType == null)
				return null;

			var assembly = Assembly.GetAssembly(interfaceType);

			return assembly == null
				? null
				: interfaceType.GetTypesImplementingThisInterface(assembly);
		}

	}
}
