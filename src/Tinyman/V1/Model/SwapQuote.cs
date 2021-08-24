using System;

namespace Tinyman.V1.Model {

	public class SwapQuote {

		public SwapType SwapType { get; internal set; }

		public AssetAmount AmountIn { get; internal set; }

		public AssetAmount AmountOut { get; internal set; }

		public AssetAmount SwapFees { get; internal set; }

		public double Slippage { get; internal set;	}

		public AssetAmount AmountOutWithSlippage {
			get {
				if (SwapType == SwapType.FixedOutput) {
					return AmountOut;
				}

				if (SwapType == SwapType.FixedInput) {
					return AmountOut - (AmountOut * Slippage);
				}

				throw new ArgumentOutOfRangeException("Invalid SwapType.");
			}
		}

		public AssetAmount AmountInWithSlippage {
			get {
				if (SwapType == SwapType.FixedInput) {
					return AmountIn;
				}

				if (SwapType == SwapType.FixedOutput) {
					return AmountIn + (AmountIn * Slippage);
				}

				throw new ArgumentOutOfRangeException("Invalid SwapType.");
			}
		}

		public double Price {
			get => AmountOut.Amount / (double)AmountIn.Amount;
		}

		public double PriceWithSlippage {
			get => AmountOutWithSlippage.Amount / (double)AmountInWithSlippage.Amount;
		}

		internal SwapQuote() { }

	}

}
