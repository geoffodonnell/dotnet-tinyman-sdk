using Algorand;
using Algorand.V2.Model;
using System;
using Tinyman.V1.Action;
using Tinyman.V1.Model;
using Account = Algorand.Account;

namespace Tinyman.V1 {

	public  static class TinymanClientExtensions {

		public static PostTransactionsResponse OptIn(this TinymanClient client, Account account) {

			var appId = client.ValidatorAppId;
			var algodApi = client.AlgodApi;
			var txParams = algodApi.TransactionParams();
			var txs = TinymanTransaction.PrepareAppOptinTransactions(
				appId, account.Address, txParams);

			txs.Sign(account);

			return client.Submit(txs, true);
		}

		public static PostTransactionsResponse OptOut(this TinymanClient client, Account account) {

			var appId = client.ValidatorAppId;
			var algodApi = client.AlgodApi;
			var txParams = algodApi.TransactionParams();
			var txs = TinymanTransaction.PrepareAppOptoutTransactions(
				appId, account.Address, txParams);

			txs.Sign(account);

			return client.Submit(txs, true);
		}

		public static PostTransactionsResponse Burn(
			this TinymanClient client, Account account, Burn action) {

			throw new NotImplementedException("'Burn' is not implemented yet.");
		}

		public static PostTransactionsResponse Mint(
			this TinymanClient client, Account account, Mint action) {

			throw new NotImplementedException("'Mint' is not implemented yet.");
		}

		public static PostTransactionsResponse Swap(
			this TinymanClient client, Account account, Swap action) {

			var txs = PrepareSwapTransactions(
				client, account.Address, action);

			txs.Sign(account);

			return client.Submit(txs, true);
		}


		public static TransactionGroup PrepareSwapTransactions(
			this TinymanClient client,
			Address sender,
			Swap action) {

			var liquidityAsset = client.FetchAsset(action.Pool.LiquidityAssetId);
			var txParams = client.AlgodApi.TransactionParams();
			
			var result = TinymanTransaction.PrepareSwapTransactions(
				client.ValidatorAppId,
				action.AmountIn,
				action.AmountOut,
				liquidityAsset,
				action.SwapType,
				sender,
				txParams);

			return result;
		}

	}

}
