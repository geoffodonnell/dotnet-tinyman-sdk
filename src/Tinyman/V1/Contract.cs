using Algorand;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Tinyman.V1.Asc;

namespace Tinyman.V1 {

	public static class Contract {

		private const string mResourceFileName = "Tinyman.V1.asc.json";
		private const string mPoolLogicSigName = "pool_logicsig";
		private const string mValidatorAppName = "validator_app";
		private const string mResourceFileLoadErrorFormat =
			"An error occured while loading the embedded resource file: '{0}'";

		private static ContractCollection mContracts;
		private static LogicSigContract mPoolLogicSigDef;
		private static AppContract mValidatorAppDef;

		private static readonly object mLock = new object();

		public static byte[] ValidatorAppApprovalProgramBytes {
			get {

				Initialize();

				return Util.GetProgram(mValidatorAppDef.ApprovalProgram, null);
			}
		}

		public static byte[] ValidatorAppClearProgramBytes {
			get {
				Initialize();
				
				return Util.GetProgram(mValidatorAppDef.ClearProgram, null);
			}
		}

		public static LogicsigSignature GetPoolLogicsigSignature(
			ulong validatorAppId, long assetIdA, long assetIdB, bool usePatch = true) {

			Initialize();

			var assetIdMax = Math.Max(assetIdA, assetIdB);
			var assetIdMin = Math.Min(assetIdA, assetIdB);

			var bytes = Util.GetProgram(mPoolLogicSigDef.Logic, new Dictionary<string, object> {
				{ "validator_app_id", validatorAppId },
				{ "asset_id_1", assetIdMax },
				{ "asset_id_2", assetIdMin }
			});

			return GetLogicsigSignature(bytes, usePatch);
		}

		private static void Initialize() {

			if (mContracts != null) {
				return;
			}

			lock (mLock) {
				if (mContracts == null) {
					LoadContractsFromResource();
				}
			}
		}

		private static LogicsigSignature GetLogicsigSignature(byte[] bytes, bool usePatch) {

			if (TryCreateLogicsigSignature(bytes, out var result, out var exception)) {
				return result;
			}

			if (usePatch && TryCreateLogicsigSignatureWithPatch(bytes, out result, out exception)) {
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

		private static bool TryCreateLogicsigSignatureWithPatch(
			byte[] bytes, out LogicsigSignature result, out Exception exception) {

			try {

				result = new LogicsigSignature {
					logic = bytes
				};

				Patch.Logic.CheckProgram(bytes, null);

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

			var stream = assembly.GetManifestResourceStream(mResourceFileName);

			if (stream == null) {
				throw new Exception(
					String.Format(mResourceFileLoadErrorFormat, mResourceFileName));
			}			
			
			try {
				using (var reader = new StreamReader(stream))
				using (var json = new JsonTextReader(reader)) {
					mContracts = serializer.Deserialize<ContractCollection>(json);
				}
			} catch (Exception ex) {
				throw new Exception(
					String.Format(mResourceFileLoadErrorFormat, mResourceFileName), ex);
			}

			stream.Dispose();

			mPoolLogicSigDef = mContracts.Contracts[mPoolLogicSigName] as LogicSigContract;
			mValidatorAppDef = mContracts.Contracts[mValidatorAppName] as AppContract;
		}

	}

}
