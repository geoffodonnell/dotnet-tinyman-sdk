﻿using Algorand;
using Tinyman.V1.Model;

namespace Tinyman.V1.Action {

	public class Redeem {

		public AssetAmount Amount { get; internal set; }

		public Address PoolAddress { get; internal set; }

		public Pool Pool { get; internal set; }

		internal Redeem() { }

		public static Redeem FromQuote(RedeemQuote quote) {
			return new Redeem {
				Amount = quote.Amount,
				PoolAddress = quote.PoolAddress,
				Pool = quote.Pool
			};
		}

	}

}
