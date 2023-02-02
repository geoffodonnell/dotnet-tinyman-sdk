using System;
using Tinyman.Model;

namespace Tinyman.V2.Model {

	/// <summary>
	/// Tinyman single asset burn quote
	/// </summary>
	public class SingleAssetBurnQuote : IQuote {

		/// <summary>
		/// Amount to receive in exchange for the liquidity asset amount (without slippage).
		/// </summary>
		public virtual AssetAmount AmountOut { get; internal set; }

		/// <summary>
		/// Liquidity asset amount
		/// </summary>
		public virtual AssetAmount LiquidityAssetAmount { get; internal set; }

		/// <summary>
		/// Slippage as a decimal fraction, e.g. 1% slippage would be expressed as 0.01.
		/// </summary>
		public virtual double Slippage { get; internal set; }

		/// <summary>
		/// Price impact as a decimal fraction, e.g. 1% slippage would be expressed as 0.01.
		/// </summary>
		public virtual double? PriceImpact { get => SwapQuote?.PriceImpact; }

		/// <summary>
		/// Swap quote
		/// </summary>
		public virtual SwapQuote SwapQuote { get; internal set; }

		/// <summary>
		/// Amount to receive (including slippage) in exchange for the liquidity asset amount.
		/// </summary>
		public virtual AssetAmount AmountOutWithSlippage {
			get {
				return AmountOut - AmountOut * Slippage;
			}
		}

		/// <inheritdoc />
		public virtual ulong ValidatorApplicationId { get; internal set; }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		public SingleAssetBurnQuote() { }

	}

}
