using Algorand;
using Algorand.Common.Asc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tinyman.V1 {

	public static class Contract {

		private const string mResourceFileNameV1_0 = "Tinyman.V1.asc.v1_0.json";
		private const string mResourceFileNameV1_1 = "Tinyman.V1.asc.v1_1.json";
		private const string mPoolLogicSigName = "pool_logicsig";
		private const string mValidatorAppName = "validator_app";
		private const string mResourceFileLoadErrorFormat =
			"An error occured while loading the embedded resource file: '{0}'";

		private static LogicSigContract mPoolLogicSigDefV1_0;
		private static LogicSigContract mPoolLogicSigDefV1_1;

		private static readonly object mLock = new object();
		private static bool mIsInitialized;

		static Contract() {
			mIsInitialized = false;
		}

		public static string GetPoolAddress(
			ulong validatorAppId, ulong assetIdA, ulong assetIdB) {

			var lsig = GetPoolLogicsigSignature(
				validatorAppId, assetIdA, assetIdB);

			return lsig?.Address?.EncodeAsString();
		}

		public static LogicsigSignature GetPoolLogicsigSignature(
			ulong validatorAppId, ulong assetIdA, ulong assetIdB) {

			Initialize();

			var logic = default(ProgramLogic);
			var assetIdMax = Math.Max(assetIdA, assetIdB);
			var assetIdMin = Math.Min(assetIdA, assetIdB);
			
			if (validatorAppId == Constant.MainnetValidatorAppIdV1_0 ||
				validatorAppId == Constant.TestnetValidatorAppIdV1_0) {

				logic = mPoolLogicSigDefV1_0.Logic;
			} else {
				logic = mPoolLogicSigDefV1_1.Logic;
			}

			var bytes = Util.GetProgram(logic, new Dictionary<string, object> {
				{ "validator_app_id", validatorAppId },
				{ "asset_id_1", assetIdMax },
				{ "asset_id_2", assetIdMin }
			});

			return GetLogicsigSignature(bytes);
		}

		private static void Initialize() {

			if (mIsInitialized) {
				return;
			}

			lock (mLock) {
				if (!mIsInitialized) {
					LoadContractsFromResource();
					mIsInitialized = true;
				}
			}
		}

		private static LogicsigSignature GetLogicsigSignature(byte[] bytes) {

			if (TryCreateLogicsigSignature(bytes, out var result, out var exception)) {
				return result;
			}

			throw exception;
		}

		private static bool TryCreateLogicsigSignature(
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

		private static void LoadContractsFromResource() {

			mPoolLogicSigDefV1_0 = LoadPoolLogicSig(mResourceFileNameV1_0);
			mPoolLogicSigDefV1_1 = LoadPoolLogicSig(mResourceFileNameV1_1);
		}

		private static LogicSigContract LoadPoolLogicSig(string fileName) {

			var assembly = typeof(Contract).Assembly;
			var serializer = JsonSerializer.CreateDefault();

			var stream = assembly.GetManifestResourceStream(fileName);

			if (stream == null) {
				throw new Exception(
					String.Format(mResourceFileLoadErrorFormat, fileName));
			}

			ContractCollection contracts = null;

			try {
				using (var reader = new StreamReader(stream))
				using (var json = new JsonTextReader(reader)) {
					contracts = serializer.Deserialize<ContractCollection>(json);
				}
			} catch (Exception ex) {
				throw new Exception(
					String.Format(mResourceFileLoadErrorFormat, fileName), ex);
			}

			stream.Dispose();

			return contracts.Contracts[mPoolLogicSigName] as LogicSigContract;
		}

	}

}
