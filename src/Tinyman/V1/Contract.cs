using Algorand;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Tinyman.V1.Asc;

namespace Tinyman.V1 {

	public static class Contract {

		private const string ResourceFileName = "Tinyman.V1.asc.json";
		private const string PoolLogicSigName = "pool_logicsig";
		private const string ValidatorAppName = "validator_app";

		private static ContractCollection mContracts;
		private static LogicSigContract PoolLogicSigDef;
		private static AppContract ValidatorAppDef;

		static Contract() {
			LoadContractsFromResource();
		}

		public static LogicsigSignature GetPoolLogicSig(
			long validatorAppId, long assetIdA, long assetIdB) {

			var assetIdMax = Math.Max(assetIdA, assetIdB);
			var assetIdMin = Math.Min(assetIdA, assetIdB);

			var bytes = Util.GetProgram(PoolLogicSigDef.Logic, new Dictionary<string, object> {
				{ "validator_app_id", validatorAppId },
				{ "asset_id_1", assetIdMax },
				{ "asset_id_2", assetIdMin }
			});

			// TODO: This is currently not supported by the Algorand SDK -- Something to do with TEAL 4?
			return new LogicsigSignature(logic: bytes);
		}

		private static void LoadContractsFromResource() {

			var assembly = typeof(Contract).Assembly;
			var serializer = JsonSerializer.CreateDefault();

			using (var stream = assembly.GetManifestResourceStream(ResourceFileName))
			using (var reader = new StreamReader(stream))
			using (var json = new JsonTextReader(reader)) {
				mContracts = serializer.Deserialize<ContractCollection>(json);
			}

			PoolLogicSigDef = mContracts.Contracts[PoolLogicSigName] as LogicSigContract;
			ValidatorAppDef = mContracts.Contracts[ValidatorAppName] as AppContract;
		}

	}

}
