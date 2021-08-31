using Algorand;
using Algorand.V2.Model;
using System;
using System.Linq;
using Tinyman.V1.Action;
using Tinyman.V1.Model;
using Account = Algorand.Account;
using Asset = Tinyman.V1.Model.Asset;

namespace Tinyman.V1 {

	public  static class TinymanClientExtensions {

		public static PostTransactionsResponse OptIn(this TinymanClient client, Account account) {

			var txs = PrepareOptInTransactions(
				client, account.Address);
			
			txs.Sign(account);

			return client.Submit(txs, true);
		}

		public static PostTransactionsResponse OptOut(this TinymanClient client, Account account) {

			var txs = PrepareOptOutTransactions(
				client, account.Address);

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

		/// <summary>
		/// Convenience method for retreiving an asset balance for an account 
		/// </summary>
		/// <param name="client">Tinyman V1 client</param>
		/// <param name="address">Account address</param>
		/// <param name="asset">Asset</param>
		/// <returns>Asset amount</returns>
		public static AssetAmount GetBalance(
			this TinymanClient client, Address address, Asset asset) {

			var info = client.AlgodApi.AccountInformation(address.EncodeAsString());

			if (asset.Id == 0) {
				return new AssetAmount(asset, Convert.ToUInt64(info.AmountWithoutPendingRewards));
			}

			var amt = info?.Assets?
				.Where(s => s.AssetId == asset.Id)
				.Select(s => s.Amount)
				.FirstOrDefault() ?? 0;

			return new AssetAmount(asset, amt);
		}

		public static TransactionGroup PrepareOptInTransactions(
			this TinymanClient client,
			Address sender) {

			var txParams = client.AlgodApi.TransactionParams();

			var result = TinymanTransaction.PrepareAppOptinTransactions(
				client.ValidatorAppId,
				sender,
				txParams);

			return result;
		}

		public static TransactionGroup PrepareOptOutTransactions(
			this TinymanClient client,
			Address sender) {

			var txParams = client.AlgodApi.TransactionParams();

			var result = TinymanTransaction.PrepareAppOptoutTransactions(
				client.ValidatorAppId,
				sender,
				txParams);

			return result;
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
