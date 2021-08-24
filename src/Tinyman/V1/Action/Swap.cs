using Tinyman.V1.Model;

namespace Tinyman.V1.Action {

	public class Swap {

		public AssetAmount AmountIn { get; set; }

		public AssetAmount AmountOut { get; set; }

		public SwapType SwapType { get; set; }

		public Swap() { }

		public static Swap FromQuote(SwapQuote quote) {
			return new Swap {
				AmountIn = quote.AmountInWithSlippage,
				AmountOut = quote.AmountOutWithSlippage,
				SwapType = quote.SwapType			
			};
		}

	}

}
