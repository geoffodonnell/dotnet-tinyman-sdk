using Algorand;
using Algorand.V2;
using Algorand.V2.Algod;
using Algorand.V2.Algod.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tinyman.Patch;
using Account = Algorand.Account;
using LogicsigSignature = Algorand.LogicsigSignature;
using SignedTransaction = Algorand.SignedTransaction;
using Transaction = Algorand.Transaction;

namespace Tinyman.V1.Model {

	public class TransactionGroup {

		public virtual Transaction[] Transactions { get; }

		public virtual SignedTransaction[] SignedTransactions { get; }

		public virtual bool IsSigned => SignedTransactions.All(s => s != null);

		public TransactionGroup(IEnumerable<Transaction> transactions)
			: this(transactions, true) { }

		public TransactionGroup(IEnumerable<Transaction> transactions, bool usePatch) {

			Transactions = transactions.Select(s => usePatch ? PatchTransaction.Create(s) : s).ToArray();
			SignedTransactions = new SignedTransaction[Transactions.Length];

			var gid = TxGroup.ComputeGroupID(Transactions);

			foreach (var tx in Transactions) {
				tx.AssignGroupID(gid);
			}
		}

		public virtual void Sign(Account account) {

			PerformSign(account.Address, s => account.SignTransaction(s));
		}

		public virtual void SignWithLogicSig(LogicsigSignature logicsig) {

			PerformSign(logicsig.Address, s => SignLogicsigTransaction(logicsig, s));
		}

		public virtual void SignWithPrivateKey(byte[] privateKey) {

			var account = Account.AccountFromPrivateKey(privateKey);

			PerformSign(account.Address, s => account.SignTransaction(s));
		}

		internal async Task<PostTransactionsResponse> SubmitAsync(DefaultApi client, bool wait = true) {
			
			if (!IsSigned) {
				throw new Exception(
					"Transaction group has not been signed.");
			}

			var bytes = new List<byte>();

			foreach (var tx in SignedTransactions) {
				 bytes.AddRange(Algorand.Encoder.EncodeToMsgPack(tx));
			}

			var payload = new MemoryStream(bytes.ToArray());
			var response = await client.TransactionsAsync(payload);

			if (wait) {
				await Util.WaitForConfirmation(client, response.TxId);
			}

			return response;
		}

		protected virtual void PerformSign(
			Address sender, Func<Transaction, SignedTransaction> action) {

			if (Transactions == null || Transactions.Length == 0) {
				return;
			}
			
			for (var i = 0; i < Transactions.Length; i++) {
				if (Transactions[i].sender.Equals(sender)) {
					var signed = action(Transactions[i]);
					SignedTransactions[i] = signed;
				}
			}
		}

		private static SignedTransaction SignLogicsigTransaction(
			LogicsigSignature logicsig, Transaction tx) {

			try {
				return Account.SignLogicsigTransaction(logicsig, tx);
			} catch (Exception ex) {
				if (tx.sender.Equals(logicsig.Address)) {
					return new SignedTransaction(tx, logicsig, tx.TxID());
				}

				throw;
			}			
		}

	}

}
