using System;

namespace Tinyman.Model {

	public class BurnQuote : IQuote {

		public virtual Tuple<AssetAmount, AssetAmount> AmountsOut { get; internal set; }

		public virtual AssetAmount LiquidityAssetAmount { get; internal set; }

		public virtual double Slippage { get; internal set; }

		public virtual Tuple<AssetAmount, AssetAmount> AmountsOutWithSlippage {
			get {
				return AmountsOut.Select(s => s - s * Slippage);
			}
		}

		public virtual ulong ValidatorApplicationId { get; internal set; }

		public BurnQuote() { }

	}

}
