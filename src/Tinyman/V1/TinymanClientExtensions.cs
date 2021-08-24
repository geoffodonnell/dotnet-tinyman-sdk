using Algorand;
using Tinyman.V1.Action;
using Tinyman.V1.Model;

namespace Tinyman.V1 {

	public  static class TinymanClientExtensions {
		
		public static TransactionGroup PrepareSwapTransactions(
			this TinymanClient client,
			Pool pool,
			Address sender,
			Swap action) {

			var liquidityAsset = client.FetchAsset(pool.LiquidityAssetId);
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
