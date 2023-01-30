using System;

namespace Tinyman.Model {

	/// <summary>
	/// Tinyman burn quote.
	/// </summary>
	public class BurnQuote : IQuote {

		/// <summary>
		/// Amounts to receive in exchange for the liquidity asset amount (without slippage).
		/// </summary>
		public virtual Tuple<AssetAmount, AssetAmount> AmountsOut { get; internal set; }

		/// <summary>
		/// Liquidity asset amount
		/// </summary>
		public virtual AssetAmount LiquidityAssetAmount { get; internal set; }

		/// <summary>
		/// Slippage as a decimal fraction, e.g. 1% slippage would be expressed as 0.01.
		/// </summary>
		public virtual double Slippage { get; internal set; }

		/// <summary>
		/// Amounts to receive (including slippage) in exchange for the liquidity asset amount.
		/// </summary>
		public virtual Tuple<AssetAmount, AssetAmount> AmountsOutWithSlippage {
			get {
				return AmountsOut.Select(s => s - s * Slippage);
			}
		}

		/// <inheritdoc />
		public virtual ulong ValidatorApplicationId { get; internal set; }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		public BurnQuote() { }

	}

}
