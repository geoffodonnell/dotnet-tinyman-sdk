using Algorand.Algod.Model;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
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

		public static byte[] IntToBytes(ulong value) {

            var result = new byte[8];

            BinaryPrimitives.WriteUInt64BigEndian(result, value);

            return result;
        }

        public static byte[] IntToBytes(long value) {

            var result = new byte[8];

            BinaryPrimitives.WriteInt64BigEndian(result, value);

            return result;
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

        public static string GetStateBytes(
            Dictionary<string, TealValue> state, string key) {

            if (state.TryGetValue(key, out var value)) {
                return value.Bytes;
            } else if (state.TryGetValue(EncodeKey(key), out value)) {
                return value.Bytes;
            }

            return null;
        }
        
        public static string IntToStateKey(ulong value) {

            return IntToStateKey(Convert.ToInt64(value));
        }

        public static string IntToStateKey(long value) {

            var paddingBytes = Encoding.UTF8.GetBytes("o");
            var valueBytes = IntToBytes(value);
            var bytes = new byte[paddingBytes.Length + valueBytes.Length];

            Array.Copy(paddingBytes, 0, bytes, 0, paddingBytes.Length);
			Array.Copy(valueBytes, 0, bytes, paddingBytes.Length, valueBytes.Length);

            var result = Base64.ToBase64String(bytes);

            return result;
        }

        public static string EncodeKey(string key) {

            var bytes = Strings.ToUtf8ByteArray(key);
            return Base64.ToBase64String(bytes);
        }

    }

}
