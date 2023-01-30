using Algorand;

namespace Tinyman.Model {

	/// <summary>
	/// Tinyman redeem quote.
	/// </summary>
	public class RedeemQuote : IQuote {

		/// <summary>
		/// Amount to redeem.
		/// </summary>
		public virtual AssetAmount Amount { get; internal set; }

		/// <summary>
		/// Pool address.
		/// </summary>
		public virtual Address PoolAddress { get; internal set; }

		/// <summary>
		/// Pool asset 1.
		/// </summary>
		public virtual Asset Asset1 { get; internal set; }

		/// <summary>
		/// Pool asset 2.
		/// </summary>
		public virtual Asset Asset2 { get; internal set; }

		/// <summary>
		/// Pool liquidity asset.
		/// </summary>
		public virtual Asset LiquidityAsset { get; internal set; }

		/// <inheritdoc />
		public virtual ulong ValidatorApplicationId { get; internal set; }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		public RedeemQuote() { }

	}

}
