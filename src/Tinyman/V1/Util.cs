using Algorand.Common;
using Algorand.Common.Asc;
using Algorand.Algod;
using Algorand.Algod.Model;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorand.Algod.Model.Transactions;

namespace Tinyman.V1 {

	public static class Util {
               
        internal static byte[] GetProgram(
            ProgramLogic logic, Dictionary<string, object> variables) {

            var template = logic.ByteCode;
            var templateBytes = Base64.Decode(template).ToList();
            
            if (variables == null) {
                return templateBytes.ToArray();
            }

            var offset = 0;

            foreach (var variable in logic.Variables.OrderBy(s => s.Index)) {

                var name = variable.Name.Substring(5).ToLower();
                var value = variables[name];
                var start = variable.Index - offset;
                var end = start + variable.Length;
                var valueEncoded = EncodeValue(value, variable.Type);
                var valueEncodedLength = valueEncoded.Length;
                var diff = variable.Length - valueEncodedLength;
                offset += diff;

                templateBytes.RemoveRange(start, variable.Length);
                templateBytes.InsertRange(start, valueEncoded);
            }

            return templateBytes.ToArray();
        }

        public static byte[] EncodeValue(object value, string type) {

            if (String.Equals(type, "int", StringComparison.OrdinalIgnoreCase)) {
                return EncodeInt(value);
            }

            throw new ArgumentException($"Unsupported value type {type}");
        }

        public static byte[] EncodeInt(object value) {

            var result = new List<byte>();
            var number = Convert.ToUInt64(value);

            while (true) {
                var toWrite = number & 0x7F;

                number >>= 7;

                if (number > 0) {
                    result.Add((byte)(toWrite | 0x80));
                } else {
                    result.Add((byte)toWrite);
                    break;
                }
            }

            return result.ToArray();
        }

        [Obsolete("This method will be removed in future versions of this library.")]
        public static async Task<PostTransactionsResponse> SignAndSubmitTransactions(
            DefaultApi client,
            Transaction[] transactions,
            List<SignedTransaction> signedTransactions,
            Account account,
            bool wait = true) {

            Algorand.TxGroup.AssignGroupID(transactions);

            for (var i = 0; i < transactions.Length; i++) {

                var tx = transactions[i];

                if (tx.Sender.Equals(account.Address)) {                    
                    signedTransactions[i] = tx.Sign(account);
                }
            }

            var response = await client.TransactionsAsync(signedTransactions);

			if (wait) {
                await client.WaitForTransactionToComplete(response.Txid);
            }

            return response;
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
            var bytes = Join(paddingBytes, valueBytes);
            var result = Base64.ToBase64String(bytes);

            return result;
        }

        public static string EncodeKey(string key) {

            var bytes = Strings.ToUtf8ByteArray(key);
            return Base64.ToBase64String(bytes);
        }

        public static T[] Join<T>(IEnumerable<T> a, IEnumerable<T> b) {

            var result = a.ToList();

            result.AddRange(b);

            return result.ToArray();
        }

    }

}
