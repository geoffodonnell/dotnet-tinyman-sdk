using Algorand;
using Algorand.Client;
using Algorand.V2;
using Algorand.V2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Account = Algorand.Account;
using LogicsigSignature = Algorand.LogicsigSignature;
using SignedTransaction = Algorand.SignedTransaction;
using Transaction = Algorand.Transaction;

namespace Tinyman.V1.Model {

	public class TransactionGroup {

		private readonly Transaction[] mTransactions;
		private SignedTransaction[] mSignedTransactions;
		//private Digest mGroupId;

		public bool IsSigned => mSignedTransactions.All(s => s != null);

		public TransactionGroup(IEnumerable<Transaction> transactions) {

			mTransactions = transactions.ToArray();
			mSignedTransactions = new SignedTransaction[mTransactions.Length];

			var gid = Algorand.TxGroup.ComputeGroupID(mTransactions);

			foreach (var tx in mTransactions) {
				tx.AssignGroupID(gid);
			}
		}

		public void Add(Transaction value) {

			//mSignedTransactions = null;
			//mGroupId = null;
			//mTransactions.Add(value);
		}

		public void Remove(Transaction value) {

			//mSignedTransactions = null;
			//mGroupId = null;
			//mTransactions.Remove(value);
		}

		public void Sign(Account account) {

			PerformSign(account.Address, s => account.SignTransaction(s));
		}

		public void SignWithLogicSig(LogicsigSignature logicsig) {

			PerformSign(logicsig.Address, s => SignLogicsigTransaction(logicsig, s));
		}

		public void SignWithPrivateKey(byte[] privateKey) {

			var account = Account.AccountFromPrivateKey(privateKey);

			PerformSign(account.Address, s => account.SignTransaction(s));
		}

		internal PostTransactionsResponse Submit(AlgodApi algodApi, bool wait = false) {
			
			if (!IsSigned) {
				throw new Exception(
					"Transaction group has not been signed. Note that adding or removing transactions will remove all signed transactions in the group.");
			}

			var bytes = new List<byte>();

			foreach (var tx in mSignedTransactions) {
				 bytes.AddRange(Algorand.Encoder.EncodeToMsgPack(tx));
			}

			//var tmp4 = JsonConvert.SerializeObject(mSignedTransactions, new JsonSerializerSettings() {
			//	DefaultValueHandling = DefaultValueHandling.Ignore,
			//	ContractResolver = AlgorandContractResolver.Instance,
			//	Formatting = Formatting.None
			//});

			var tmp8 = JsonConvert.SerializeObject(
				Algorand.Encoder.DecodeFromMsgPack<JToken>(bytes.ToArray()));

			var tmp12 = $"[{tmp8}]";
			
			ApiResponse<PostTransactionsResponse> response;
			
			try {

				//var request = WebRequest.CreateHttp(@"https://api.testnet.algoexplorer.io/v2/transactions");

				//request.Method = "POST";
				//request.Headers[HttpRequestHeader.ContentType] = "application/x-binary";
				//request.Headers[HttpRequestHeader.ContentLength] = bytes.Count.ToString();
				//request.Headers[HttpRequestHeader.AcceptEncoding] = "identity";
				//request.Headers[HttpRequestHeader.UserAgent] = "algosdk";
				//request.Headers[HttpRequestHeader.Connection] = "close";
				//request.Headers["X-Algo-Api-Token"] = null;

				//request.KeepAlive = false;

				//using (var body = request.GetRequestStream()) {
				//	body.Write(bytes.ToArray(), 0, bytes.Count);
				//	//body.Flush();
				//}

				//using (var res = request.GetResponse())
				//using (var body = res.GetResponseStream())
				//using (var reader = new StreamReader(body)) {

				//	var tmp1 = reader.ReadToEnd();
				//	Console.WriteLine(tmp1);

				//}
					






				response = algodApi.RawTransactionWithHttpInfo(bytes.ToArray());

				if (wait) {
					Algorand.Utils.WaitTransactionToComplete(algodApi, response.Data.TxId);
				}

				return response.Data;

			} catch(Exception ex) {
				return null;
			}
		}

		protected virtual void PerformSign(
			Address sender, Func<Transaction, SignedTransaction> action) {

			if (mTransactions == null || mTransactions.Length == 0) {
				return;
			}
			
			for (var i = 0; i < mTransactions.Length; i++) {
				if (mTransactions[i].sender.Equals(sender)) {
					var signed = action(mTransactions[i]);
					mSignedTransactions[i] = signed;
				}
			}
		}

		private static SignedTransaction SignLogicsigTransaction(
			LogicsigSignature logicsig, Transaction tx) {

			try {
				return Account.SignLogicsigTransaction(logicsig, tx);
			} catch (Exception ex) {
				if (tx.sender.Equals(logicsig.Address)) {
					var stx = new SignedTransaction(tx, logicsig, tx.TxID());
					//stx.SetAuthAddr(tx.sender.Bytes);

					//var tmp1 = Encoder.EncodeToMsgPack(stx);
					//var tmp2 = Encoder.DecodeFromMsgPack<SignedTransaction>(tmp1);



					return stx;
				}

				throw;
			}
			
		}

	}

}
