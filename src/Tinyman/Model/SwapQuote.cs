using System;

namespace Tinyman.Model {

	/// <summary>
	/// Tinyman swap quote.
	/// </summary>
	public class SwapQuote : IQuote {

		/// <summary>
		/// Swap type.
		/// </summary>
		public virtual SwapType SwapType { get; internal set; }

		/// <summary>
		/// Swap amount in (without slippage).
		/// </summary>
		public virtual AssetAmount AmountIn { get; internal set; }

		/// <summary>
		/// Swap amount out (without slippage).
		/// </summary>
		public virtual AssetAmount AmountOut { get; internal set; }

		/// <summary>
		/// Swap fees.
		/// </summary>
		public virtual AssetAmount SwapFees { get; internal set; }

		/// <summary>
		/// Slippage as a decimal fraction, e.g. 1% slippage would be expressed as 0.01.
		/// </summary>
		public virtual double Slippage { get; internal set; }

		/// <summary>
		/// Price impact as a decimal fraction, e.g. 1% slippage would be expressed as 0.01.
		/// </summary>
		public virtual double? PriceImpact { get; internal set; }

		/// <summary>
		/// Liquidity asset.
		/// </summary>
		public virtual Asset LiquidityAsset { get; internal set; }

		/// <summary>
		/// Swap output out with slippage.
		/// </summary>
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

		/// <summary>
		/// Swap input with slippage.
		/// </summary>
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

		/// <summary>
		/// Swap price (without slippage).
		/// </summary>
		public virtual double Price {
			get => AmountOut.Amount / (double)AmountIn.Amount;
		}

		/// <summary>
		/// Swap price with slippage.
		/// </summary>
		public virtual double PriceWithSlippage {
			get => AmountOutWithSlippage.Amount / (double)AmountInWithSlippage.Amount;
		}
		
		/// <inheritdoc />
		public virtual ulong ValidatorApplicationId { get; internal set; }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		public SwapQuote() { }

	}

}
