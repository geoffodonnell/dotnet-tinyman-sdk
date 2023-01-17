using Algorand;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Tinyman.V2 {

	public static class Contract {

		private static byte[] mPoolLogicSigTemplate;
		private static bool mIsInitialized;

		private static readonly object mLock = new ();
		private static readonly Dictionary<string, Address> mPoolAddressCache = new ();

		static Contract() {
			mIsInitialized = false;
		}

		/// <summary>
		/// Get a pool address
		/// </summary>
		/// <param name="validatorAppId">Validator application ID</param>
		/// <param name="assetIdA">Asset A ID</param>
		/// <param name="assetIdB">Asset B ID</param>
		/// <returns>Pool address</returns>
		/// <remarks>Pool addresses are internally cached. Call <see cref="ClearPoolDataCache"/> to empty the cache.</remarks>
		public static Address GetPoolAddress(
			ulong validatorAppId, ulong assetIdA, ulong assetIdB) {

			var assetIdMax = Math.Max(assetIdA, assetIdB);
			var assetIdMin = Math.Min(assetIdA, assetIdB);
			var key = $"{validatorAppId}-{assetIdMax}-{assetIdMin}";

			if (mPoolAddressCache.TryGetValue(key, out var result)) {
				return result;
			}

			Initialize();

			var lsig = GetPoolLogicsigSignatureUnchecked(
				validatorAppId, assetIdMax, assetIdMin);

			result = lsig?.Address;

			mPoolAddressCache[key] = result;

			return result;
		}

		/// <summary>
		/// Get a pool logicsig signature
		/// </summary>
		/// <param name="validatorAppId">Validator application ID</param>
		/// <param name="assetIdA">Asset A ID</param>
		/// <param name="assetIdB">Asset B ID</param>
		/// <returns>Pool logicsig signature</returns>
		/// <remarks>Pool logicsig signatures are NOT internally cached.</remarks>
		public static LogicsigSignature GetPoolLogicsigSignature(
			ulong validatorAppId, ulong assetIdA, ulong assetIdB) {

			Initialize();

			var assetIdMax = Math.Max(assetIdA, assetIdB);
			var assetIdMin = Math.Min(assetIdA, assetIdB);

			return GetPoolLogicsigSignatureUnchecked(validatorAppId, assetIdMax, assetIdMin);
		}

		/// <summary>
		/// Clear the pool address cache.
		/// </summary>
		public static void ClearPoolDataCache() {
			mPoolAddressCache.Clear();
		}

		private static LogicsigSignature GetPoolLogicsigSignatureUnchecked(
			ulong validatorAppId, ulong assetIdMax, ulong assetIdMin) {

			var bytes = new byte[mPoolLogicSigTemplate.Length];

			Array.Copy(ToBytes(validatorAppId), 0, bytes, 3, 8);
			Array.Copy(ToBytes(assetIdMax), 0, bytes, 11, 8);
			Array.Copy(ToBytes(assetIdMin), 0, bytes, 19, 8);

			return new LogicsigSignature(bytes);
		}

		private static void Initialize() {

			if (mIsInitialized) {
				return;
			}

			lock (mLock) {
				if (!mIsInitialized) {
					mPoolLogicSigTemplate = Base64.Decode(Constant.PoolLogicSigTemplateAsB64);
					mIsInitialized = true;
				}
			}
		}

		public static byte[] ToBytes(ulong value) {

			var result = new byte[8];

			BinaryPrimitives.WriteUInt64BigEndian(result, value);

			return result;
		}

	}

}
