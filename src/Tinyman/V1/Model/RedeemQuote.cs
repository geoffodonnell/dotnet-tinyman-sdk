using Algorand;
using System;

namespace Tinyman.V1.Model {

	public class RedeemQuote {

		public AssetAmount Amount { get; internal set; }

		public Address PoolAddress { get; internal set; }

		internal RedeemQuote() { }

	}

}
