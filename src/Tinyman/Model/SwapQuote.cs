using System;

namespace Tinyman.Model {

	public class SwapQuote {

		public virtual SwapType SwapType { get; internal set; }

		public virtual AssetAmount AmountIn { get; internal set; }

		public virtual AssetAmount AmountOut { get; internal set; }

		public virtual AssetAmount SwapFees { get; internal set; }

		public virtual double Slippage { get; internal set; }

		public virtual double? PriceImpact { get; internal set; }

		public virtual Asset LiquidityAsset { get; internal set; }

		public virtual AssetAmount AmountOutWithSlippage {
			get {
				if (SwapType == SwapType.FixedOutput) {
					return AmountOut;
				}

				if (SwapType == SwapType.FixedInput) {
					return AmountOut - AmountOut * Slippage;
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
					return AmountIn + AmountIn * Slippage;
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

	}

}
