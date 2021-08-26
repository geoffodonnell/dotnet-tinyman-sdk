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
		private const string UnsupportedVersionErrorMessageFromSdk = "unsupported version";
		private const string UnsupportedVersionErrorMessage = "Unsupported version -- use Contract.GetPoolLogicSig4 until the Algorand SDK is updated to support TEAL 4 opcodes.";

		private static ContractCollection mContracts;
		private static LogicSigContract PoolLogicSigDef;
		private static AppContract ValidatorAppDef;

		public static byte[] ValidatorAppApprovalProgramBytes {
			get => Util.GetProgram(ValidatorAppDef.ApprovalProgram, null);
		}

		public static byte[] ValidatorAppClearProgramBytes {
			get => Util.GetProgram(ValidatorAppDef.ClearProgram, null);
		}

		static Contract() {
			LoadContractsFromResource();
		}

		public static LogicsigSignature GetPoolLogicSig(
			ulong validatorAppId, long assetIdA, long assetIdB) {

			var assetIdMax = Math.Max(assetIdA, assetIdB);
			var assetIdMin = Math.Min(assetIdA, assetIdB);

			var bytes = Util.GetProgram(PoolLogicSigDef.Logic, new Dictionary<string, object> {
				{ "validator_app_id", validatorAppId },
				{ "asset_id_1", assetIdMax },
				{ "asset_id_2", assetIdMin }
			});

			return GetLogicSig(bytes);
		}

		private static LogicsigSignature GetLogicSig(byte[] bytes) {

			if (TryCreateLogicSig(bytes, out var result, out var exception)) {
				return result;
			}

			if (TryCreateLogicSigWithManualCheck(bytes, out result, out exception)) {
				return result;
			}

			throw exception;
		}

		private static bool TryCreateLogicSig(
			byte[] bytes, out LogicsigSignature result, out Exception exception) {

			try {

				result = new LogicsigSignature(logic: bytes);
				exception = null;

				return true;

			} catch (Exception ex) {

				result = null;
				exception = ex;

				return false;
			}
		}

		private static bool TryCreateLogicSigWithManualCheck(
			byte[] bytes, out LogicsigSignature result, out Exception exception) {

			try {

				result = new LogicsigSignature {
					logic = bytes
				};

				Teal4.Logic.CheckProgram(bytes, null);

				exception = null;

				return true;

			} catch (Exception ex) {

				result = null;
				exception = ex;

				return false;
			}
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
