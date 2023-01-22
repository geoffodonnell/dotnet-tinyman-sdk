using System;

namespace Tinyman.Model {

	public class MintQuote {

		public virtual Tuple<AssetAmount, AssetAmount> AmountsIn { get; internal set; }

		public virtual AssetAmount LiquidityAssetAmount { get; internal set; }

		public virtual double Slippage { get; internal set; }

		public virtual AssetAmount LiquidityAssetAmountWithSlippage {
			get {
				return LiquidityAssetAmount - LiquidityAssetAmount * Slippage;
			}
		}

		public MintQuote() { }

	}

}
