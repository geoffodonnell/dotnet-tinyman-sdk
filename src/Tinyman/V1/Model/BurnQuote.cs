using System;

namespace Tinyman.V1.Model {

	public class BurnQuote {

		public Tuple<AssetAmount, AssetAmount> AmountsOut { get; internal set; }

		public AssetAmount LiquidityAssetAmount { get; internal set; }

		public double Slippage { get; internal set; }

		internal Pool Pool { get; set; }

		public Tuple<AssetAmount, AssetAmount> AmountsOutWithSlippage {
			get {
				return AmountsOut.Select(s => s - (s * Slippage));
			}
		}

		public BurnQuote(Pool pool) {
			Pool = pool;
		}

	}

}
