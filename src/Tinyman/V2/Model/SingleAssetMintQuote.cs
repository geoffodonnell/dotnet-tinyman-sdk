using System;
using Tinyman.Model;

namespace Tinyman.V2.Model {

	/// <summary>
	/// Tinyman single asset mint quote
	/// </summary>
	public class SingleAssetMintQuote : IQuote {

		public virtual AssetAmount AmountIn { get; internal set; }

		/// <summary>
		/// Liquidity asset amount (without slippage).
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
		/// Liquidity asset amount with slippage.
		/// </summary>
		public virtual AssetAmount LiquidityAssetAmountWithSlippage {
			get {
				return LiquidityAssetAmount - LiquidityAssetAmount * Slippage;
			}
		}

		/// <inheritdoc />
		public virtual ulong ValidatorApplicationId { get; internal set; }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		public SingleAssetMintQuote() { }

	}

}
