using Algorand;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Tinyman.V1.Asc;
using Tinyman.V1.Teal4;

namespace Tinyman.V1 {

	public static class Contract {

		private const string ResourceFileName = "Tinyman.V1.asc.json";
		private const string PoolLogicSigName = "pool_logicsig";
		private const string ValidatorAppName = "validator_app";
		private const string UnsupportedVersionErrorMessageFromSdk = "unsupported version";
		private const string UnsupportedVersionErrorMessage = "Unsupported version -- use Contract.GetPoolLogicSig4 until the Algorand SDK is updated to support TEAL 4 opcodes.";

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

			try {
				return new LogicsigSignature(logic: bytes);

			} catch (ArgumentException ex) {

				if (String.Equals(ex.Message, UnsupportedVersionErrorMessageFromSdk)) {
					throw new ArgumentException(UnsupportedVersionErrorMessage, ex);
				}

				throw;
			}
		}

		public static LogicsigSignature4 GetPoolLogicSig4(
			long validatorAppId, long assetIdA, long assetIdB) {

			var assetIdMax = Math.Max(assetIdA, assetIdB);
			var assetIdMin = Math.Min(assetIdA, assetIdB);

			var bytes = Util.GetProgram(PoolLogicSigDef.Logic, new Dictionary<string, object> {
				{ "validator_app_id", validatorAppId },
				{ "asset_id_1", assetIdMax },
				{ "asset_id_2", assetIdMin }
			});

			return new LogicsigSignature4(logic: bytes);
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
