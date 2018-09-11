using System;
using System.Collections.Generic;

namespace EntityFX.Core
{
	/// <summary>
	/// These extension methods make all reference <see cref="TDCodeGenerator"/>,
	/// and make it easy for consumers of <see cref="IDbRepositoryCore{T, TId}"/>
	/// and other types to easily (/ easily discoverable) generate TableDefinition 
	/// code for EntitySets.
	/// </summary>
	public static class TDCodeGeneratorX
	{
		public static string GetTableDefinitionsCodeForEntityTypes(
			this ITableMetaInfoBuilder tmiBldr,
			IEnumerable<Type> types,
			TDCodeGenerator codeGenOps = null,
			bool withNamespace = true)
			=> codeGenOps.E().GetTableDefinitionsCodeForEntityTypes(tmiBldr, types, codeGenOps, withNamespace);

		public static string GetTableDefinitionsCodeForAllDbSets(
			this ITableMetaInfoBuilder tmiBldr,
			TDCodeGenerator codeGenOps = null,
			bool withNamespace = true) 
			=> GetTableDefinitionsCodeForEntityTypes(
				tmiBldr,
				tmiBldr.GetEntityTypes(),
				codeGenOps,
				withNamespace);

		public static string GetTableMetaInfoCode(
			this ITableMetaInfo tmi)
			=> TDCodeGenerator.GetTableMetaInfoCodeStatic(tmi);

		public static string GetTableDefinitionCode(
			this ITableMetaInfoBuilder tmiBldr,
			Type type,
			TDCodeGenerator codeGenOps = null,
			bool withNamespace = true)
			=> codeGenOps.E().GetTableDefinitionCode(tmiBldr, type, withNamespace);

		public static string GetTableDefinitionCode(
			this ITableMetaInfo tinfo,
			TDCodeGenerator codeGenOps = null,
			bool withNamespace = true)
			=> codeGenOps.E().GetTableDefinitionCode(tinfo, withNamespace);

	}
}
