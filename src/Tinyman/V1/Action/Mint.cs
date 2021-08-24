using System;
using Tinyman.V1.Model;

namespace Tinyman.V1.Action {

	public class Mint {

		public Tuple<AssetAmount, AssetAmount> Amounts { get; internal set; }

		internal Pool Pool { get; set; }

		public Mint() { }

		public static Mint FromQuote(MintQuote quote) {
			return new Mint {
				Amounts = new Tuple<AssetAmount, AssetAmount>(
					quote.AmountsIn.Item1, quote.AmountsIn.Item2),
				Pool = quote.Pool
			};
		}

	}

}
