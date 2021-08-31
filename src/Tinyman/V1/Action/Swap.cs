using Tinyman.V1.Model;

namespace Tinyman.V1.Action {

	public class Swap {

		public AssetAmount AmountIn { get; set; }

		public AssetAmount AmountOut { get; set; }

		public SwapType SwapType { get; set; }

		internal Pool Pool { get; set; }

		internal Swap() { }

		public static Swap FromQuote(SwapQuote quote) {

			var amountIn = default(AssetAmount);
			var amountOut = default(AssetAmount);

			if (quote.SwapType == SwapType.FixedInput) {
				amountIn = quote.AmountIn;
				amountOut = quote.AmountOutWithSlippage;
			} else {
				amountIn = quote.AmountInWithSlippage;
				amountOut = quote.AmountOut;
			}

			return new Swap {
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapType = quote.SwapType,
				Pool = quote.Pool
			};
		}

	}

}
