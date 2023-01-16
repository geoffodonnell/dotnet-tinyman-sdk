using System;
using Tinyman.Model;
using Tinyman.V1.Model;

namespace Tinyman.V1.Action {

    public class Burn {

		public Tuple<AssetAmount, AssetAmount> Amounts { get; internal set; }

		public AssetAmount LiquidityAssetAmount { get; internal set; }

		internal Pool Pool { get; set; }

		internal Burn() { }

		public static Burn FromQuote(BurnQuote quote) {
			return new Burn {
				Amounts = new Tuple<AssetAmount, AssetAmount>(
					quote.AmountsOutWithSlippage.Item1, quote.AmountsOutWithSlippage.Item2),
				LiquidityAssetAmount = quote.LiquidityAssetAmount,
				Pool = quote.Pool
			};
		}

	}

}
