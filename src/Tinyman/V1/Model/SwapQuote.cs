using System;

namespace Tinyman.V1.Model {

	public class SwapQuote {

		public virtual SwapType SwapType { get; set; }

		public virtual AssetAmount AmountIn { get; set; }

		public virtual AssetAmount AmountOut { get; set; }

		public virtual AssetAmount SwapFees { get; set; }

		public virtual double Slippage { get; set;	}

		internal Pool Pool { get; set; }

		public virtual AssetAmount AmountOutWithSlippage {
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

		public virtual AssetAmount AmountInWithSlippage {
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

		public virtual double Price {
			get => AmountOut.Amount / (double)AmountIn.Amount;
		}

		public virtual double PriceWithSlippage {
			get => AmountOutWithSlippage.Amount / (double)AmountInWithSlippage.Amount;
		}

		public SwapQuote(Pool pool) {
			Pool = pool;
		}

	}

}
