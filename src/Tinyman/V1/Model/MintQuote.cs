using System;

namespace Tinyman.V1.Model {

	public class MintQuote {

		public Tuple<AssetAmount, AssetAmount> AmountsIn { get; internal set; }

		public AssetAmount LiquidityAssetAmount { get; internal set; }

		public double Slippage { get; internal set; }

		internal Pool Pool { get; set; }

		public AssetAmount LiquidityAssetAmountWithSlippage {
			get {
				return LiquidityAssetAmount - (LiquidityAssetAmount * Slippage);
			}
		}

		public MintQuote(Pool pool) {
			Pool = pool;
		}

	}

}
