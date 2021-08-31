using Algorand;
using Algorand.V2;
using Algorand.V2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Account = Algorand.Account;
using LogicsigSignature = Algorand.LogicsigSignature;
using SignedTransaction = Algorand.SignedTransaction;
using Transaction = Algorand.Transaction;

namespace Tinyman.V1.Model {

	public class TransactionGroup {

		private readonly Transaction[] mTransactions;
		private SignedTransaction[] mSignedTransactions;

		public virtual bool IsSigned => mSignedTransactions.All(s => s != null);

		public TransactionGroup(IEnumerable<Transaction> transactions) {

			mTransactions = transactions.ToArray();
			mSignedTransactions = new SignedTransaction[mTransactions.Length];

			var gid = Algorand.TxGroup.ComputeGroupID(mTransactions);

			foreach (var tx in mTransactions) {
				tx.AssignGroupID(gid);
			}
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
			
			var response = algodApi.RawTransactionWithHttpInfo(bytes.ToArray());

			if (wait) {
				Algorand.Utils.WaitTransactionToComplete(algodApi, response.Data.TxId);
			}

			return response.Data;
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
					return new SignedTransaction(tx, logicsig, tx.TxID());
				}

				throw;
			}			
		}

	}

}
