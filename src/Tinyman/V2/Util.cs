using Algorand.Algod.Model;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using Tinyman.Model;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.V2 {

    /// <summary>
    /// Usefule utility methods
    /// </summary>
    internal static class Util {

        public static Tuple<Asset, Asset> EnsureOrder(Asset a, Asset b) {
            if (a.Id > b.Id) {
                return new Tuple<Asset, Asset>(a, b);
            }

            return new Tuple<Asset,Asset>(b, a);
        }

		public static Tuple<AssetAmount, AssetAmount> EnsureOrder(AssetAmount a, AssetAmount b) {
			if (a.Asset.Id > b.Asset.Id) {
				return new Tuple<AssetAmount, AssetAmount>(a, b);
			}

			return new Tuple<AssetAmount, AssetAmount>(b, a);
		}

        public static AssetAmount ForAsset(
            this Tuple<AssetAmount, AssetAmount> assets, Asset asset) {

            if (assets.Item1.Asset == asset) {
                return assets.Item1;
            } else if (assets.Item2.Asset == asset) {
				return assets.Item2;
			}

            return null;
		}

		public static AssetAmount ForOtherAsset(
	        this Tuple<AssetAmount, AssetAmount> assets, Asset asset) {

			if (assets.Item1.Asset == asset) {
				return assets.Item2;
			} else if (assets.Item2.Asset == asset) {
				return assets.Item1;
			}

			return null;
		}

        public static ulong? GetStateInt(
            Dictionary<string, TealValue> state, string key) {

            if (state.TryGetValue(key, out var value)) {
                return value.Uint;
            } else if (state.TryGetValue(EncodeKey(key), out value)) {
                return value.Uint;
            }

            return null;
        }

        public static string EncodeKey(string key) {

            var bytes = Strings.ToUtf8ByteArray(key);
            return Base64.ToBase64String(bytes);
        }

    }

}
