using System;
using Tinyman.Model;

namespace Tinyman.V2.Model {

	public class SingleAssetBurnQuote : IQuote {

		public virtual AssetAmount AmountOut { get; internal set; }

		public virtual AssetAmount LiquidityAssetAmount { get; internal set; }

		public virtual double Slippage { get; internal set; }

		public virtual double? PriceImpact { get => SwapQuote?.PriceImpact; }

		public virtual SwapQuote SwapQuote { get; internal set; }

		public virtual AssetAmount AmountOutWithSlippage {
			get {
				return AmountOut - AmountOut * Slippage;
			}
		}

		public virtual ulong ValidatorApplicationId { get; internal set; }

		public SingleAssetBurnQuote() { }

	}

}
