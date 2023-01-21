using System;

namespace Tinyman.Model {

	public class BurnQuote {

		public virtual Tuple<AssetAmount, AssetAmount> AmountsOut { get; internal set; }

		public virtual AssetAmount LiquidityAssetAmount { get; internal set; }

		public virtual double Slippage { get; internal set; }

		internal Pool Pool { get; set; }

		public virtual Tuple<AssetAmount, AssetAmount> AmountsOutWithSlippage {
			get {
				return AmountsOut.Select(s => s - s * Slippage);
			}
		}

		public BurnQuote(Pool pool) {
			Pool = pool;
		}

	}

}
