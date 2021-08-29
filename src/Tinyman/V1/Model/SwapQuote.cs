using System;

namespace Tinyman.V1.Model {

	public class SwapQuote {

		public SwapType SwapType { get; set; }

		public AssetAmount AmountIn { get; set; }

		public AssetAmount AmountOut { get; set; }

		public AssetAmount SwapFees { get; set; }

		public double Slippage { get; set;	}

		internal Pool Pool { get; set; }

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

		public SwapQuote(Pool pool) {
			Pool = pool;
		}

	}

}
