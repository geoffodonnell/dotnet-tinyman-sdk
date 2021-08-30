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

			var txs = PrepareBurnTransactions(
				client, account.Address, action);

			txs.Sign(account);

			return client.Submit(txs, true);
		}

		public static PostTransactionsResponse Mint(
			this TinymanClient client, Account account, Mint action) {

			var txs = PrepareMintTransactions(
				client, account.Address, action);

			txs.Sign(account);

			return client.Submit(txs, true);
		}

		public static PostTransactionsResponse Swap(
			this TinymanClient client, Account account, Swap action) {

			var txs = PrepareSwapTransactions(
				client, account.Address, action);

			txs.Sign(account);

			return client.Submit(txs, true);
		}

		public static PostTransactionsResponse Redeem(
			this TinymanClient client, Account account, Redeem action) {

			var txs = PrepareRedeemTransactions(
				client, account.Address, action);

			txs.Sign(account);

			return client.Submit(txs, true);
		}

		public static TransactionGroup PrepareBurnTransactions(
			this TinymanClient client,
			Address sender,
			Burn action) {

			var txParams = client.AlgodApi.TransactionParams();

			var result = TinymanTransaction.PrepareBurnTransactions(
				client.ValidatorAppId,
				action.Amounts.Item1,
				action.Amounts.Item2,
				action.LiquidityAssetAmount,
				sender,
				txParams);

			return result;
		}

		public static TransactionGroup PrepareMintTransactions(
			this TinymanClient client,
			Address sender,
			Mint action) {

			var txParams = client.AlgodApi.TransactionParams();

			var result = TinymanTransaction.PrepareMintTransactions(
				client.ValidatorAppId,
				action.Amounts.Item1,
				action.Amounts.Item2,
				action.LiquidityAssetAmount,
				sender,
				txParams);

			return result;
		}

		public static TransactionGroup PrepareSwapTransactions(
			this TinymanClient client,
			Address sender,
			Swap action) {

			var txParams = client.AlgodApi.TransactionParams();

			var result = TinymanTransaction.PrepareSwapTransactions(
				client.ValidatorAppId,
				action.AmountIn,
				action.AmountOut,
				action.Pool.LiquidityAsset,
				action.SwapType,
				sender,
				txParams);

			return result;
		}

		public static TransactionGroup PrepareRedeemTransactions(
			this TinymanClient client, Address sender, Redeem action) {

			// TODO: Store a cache instance w/o the amounts?
			if (action.Pool == null) {
				action.Pool = client.FetchPool(action.PoolAddress);
			}

			var txParams = client.AlgodApi.TransactionParams();

			var result = TinymanTransaction.PrepareRedeemTransactions(
				client.ValidatorAppId,
				action.Pool.Asset1,
				action.Pool.Asset2,
				action.Pool.LiquidityAsset,
				action.Amount,
				sender,
				txParams);

			return result;
		}

	}

}
