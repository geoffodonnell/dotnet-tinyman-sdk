using Algorand;

namespace Tinyman.Model {

	public class RedeemQuote {

		public virtual AssetAmount Amount { get; internal set; }

		public virtual Address PoolAddress { get; internal set; }

		public virtual Asset Asset1 { get; internal set; }

		public virtual Asset Asset2 { get; internal set; }

		public virtual Asset LiquidityAsset { get; internal set; }

		public RedeemQuote() { }

	}

}
