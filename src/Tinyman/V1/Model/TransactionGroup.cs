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

		private readonly List<Transaction> mTransactions;
		private List<SignedTransaction> mSignedTransactions;

		public TransactionGroup(IEnumerable<Transaction> transactions) {
			mTransactions = new List<Transaction>(transactions);
		}

		public void Add(Transaction value) {

			mSignedTransactions = null;
			mTransactions.Add(value);
		}

		public void Remove(Transaction value) {

			mSignedTransactions = null;
			mTransactions.Remove(value);
		}

		public void Sign(Account account) {

			PerformSign(s => account.SignTransaction(s));
		}

		public void SignWithLogicSig(LogicsigSignature logicsig) {

			PerformSign(s => Account.SignLogicsigTransaction(logicsig, s));
		}

		public void SignWithPrivateKey(byte[] privateKey) {

			var account = Account.AccountFromPrivateKey(privateKey);

			PerformSign(s => account.SignTransaction(s));
		}

		internal PostTransactionsResponse Submit(AlgodApi algodApi, bool wait = false) {
			
			if (mSignedTransactions == null) {
				throw new Exception("Transactions have not been signed.");
			}

			var bytes = mSignedTransactions
				.SelectMany(s => Algorand.Encoder.EncodeToMsgPack(s))
				.ToArray();

			var response = algodApi.RawTransaction(bytes);

			if (wait) {
				Algorand.Utils.WaitTransactionToComplete(algodApi, response.TxId);
			}

			return response;
		}

		protected virtual void PerformSign(Func<Transaction, SignedTransaction> action) {

			if (mTransactions == null || mTransactions.Count == 0) {
				mSignedTransactions = null;
			}

			if (mTransactions.Count == 1) {
				mSignedTransactions = mTransactions.Select(s => action(s)).ToList();
			}

			var groupId = Algorand.TxGroup.ComputeGroupID(mTransactions.ToArray());

			mTransactions.ForEach(s => s.AssignGroupID(groupId));

			mSignedTransactions = mTransactions
				.Select(s => action(s))
				.ToList();
		}

	}

}
