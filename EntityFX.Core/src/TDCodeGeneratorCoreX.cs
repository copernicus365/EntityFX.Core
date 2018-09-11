using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EntityFX.Core
{
	/// <summary>
	/// Counterpart to <see cref="TDCodeGeneratorX"/>, these extension methods 
	/// reference types that depend on EFCore (DbContext, etc), and so must live
	/// in the EntityFX.Core project (as opposed to .Base).
	/// </summary>
	public static class TDCodeGeneratorCoreX
	{
		public static ITableMetaInfoBuilder GetTableMetaInfoBuilder(
			this DbContext dbContext)
			=> new TableMetaInfoBuilderCore(dbContext);

		public static string GetTableDefinitionsCodeForAllDbSets(
			this DbContext dbContext,
			TDCodeGenerator options = null,
			bool withNamespace = true)
		{
			ITableMetaInfoBuilder tableMeta = dbContext.GetTableMetaInfoBuilder();
			string code = tableMeta.GetTableDefinitionsCodeForAllDbSets(options, withNamespace);
			return code;
		}

		public static string GetTableDefinitionsCodeForAllDbSets(
			this DbContext[] dbContexts,
			TDCodeGenerator options = null)
		{
			var tdCodes = new List<string>(dbContexts.Length);

			if (options == null)
				options = new TDCodeGenerator();

			for (int i = 0; i < dbContexts.Length; i++) {
				string tdCode = GetTableDefinitionsCodeForAllDbSets(dbContexts[i], options, withNamespace: false);
				tdCodes.Add(tdCode);
			}

			string code = options.WrapTableDefinitionsCodeInOuterNamespaceAndUsings(tdCodes);
			return code;
		}

		public static string WriteTableDefinitionsCodeForAllDbSets(
			this DbContext[] dbContexts,
			TDCodeGenerator options,
			string pathTDsCSFile,
			string pathTDsOldCommentedOutCSFile = null,
			int maxLinesToSaveOldCommentedOutVersion = 10_000)
		{
			if (options == null)
				options = new TDCodeGenerator();

			var tdCodes = new List<string>(dbContexts.Length);

			for (int i = 0; i < dbContexts.Length; i++) {
				string tdCode = GetTableDefinitionsCodeForAllDbSets(dbContexts[i], options, withNamespace: false);
				tdCodes.Add(tdCode);
			}

			string finalResult = options.WriteNewTableDefinitionsCode(
				tdCodes, 
				pathTDsCSFile, 
				pathTDsOldCommentedOutCSFile, 
				maxLinesToSaveOldCommentedOutVersion);

			return finalResult;
		}


	}
}
