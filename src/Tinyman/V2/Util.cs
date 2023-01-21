using Algorand.Algod.Model;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Tinyman.V2 {

    internal static class Util {
               
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
