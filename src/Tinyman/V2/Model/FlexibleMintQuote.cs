using System;
using Tinyman.Model;

namespace Tinyman.V2.Model {

	public class FlexibleMintQuote : IQuote {

		/// <summary>
		/// Amounts to provide to pool in exchange for the liquidity asset.
		/// </summary>
		public virtual Tuple<AssetAmount, AssetAmount> AmountsIn { get; internal set; }

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
		public virtual double? PriceImpact { get; internal set; }

		/// <summary>
		/// Swap quote
		/// </summary>
		/// <remarks>
		/// If null, the entire quote is treated as a proportional mint quote.
		/// </remarks>
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
		public FlexibleMintQuote() { }

	}

}
