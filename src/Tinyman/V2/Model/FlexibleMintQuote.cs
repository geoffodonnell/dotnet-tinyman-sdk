using System;
using Tinyman.Model;

namespace Tinyman.V2.Model {

	public class FlexibleMintQuote : IQuote {

		public virtual Tuple<AssetAmount, AssetAmount> AmountsIn { get; internal set; }

		public virtual AssetAmount LiquidityAssetAmount { get; internal set; }

		public virtual double Slippage { get; internal set; }

		public virtual double? PriceImpact { get; internal set; }

		public virtual SwapQuote SwapQuote { get; internal set; }

		public virtual AssetAmount LiquidityAssetAmountWithSlippage {
			get {
				return LiquidityAssetAmount - LiquidityAssetAmount * Slippage;
			}
		}

		public virtual ulong ValidatorApplicationId { get; internal set; }

		public FlexibleMintQuote() { }

	}

}
