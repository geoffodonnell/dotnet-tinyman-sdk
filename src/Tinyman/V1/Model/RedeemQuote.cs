using Algorand;

namespace Tinyman.V1.Model {

	public class RedeemQuote {

		public virtual AssetAmount Amount { get; internal set; }

		public virtual Address PoolAddress { get; internal set; }

		internal RedeemQuote() { }

	}

}
