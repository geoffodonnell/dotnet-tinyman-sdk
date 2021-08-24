namespace Tinyman.V1.Model {

	public class Pool {

		public string Address { get; internal set; }

		public Asset Asset1 { get; internal set; }

		public Asset Asset2 { get; internal set; }

		public long LiquidityAssetId { get; internal set; }

		public string LiquidityAssetName { get; internal set; }

		public ulong Asset1Reserves { get; internal set;  }

		public ulong Asset2Reserves { get; internal set; }

		public ulong IssuedLiquidity { get; internal set; }

		public ulong UnclaimedProtocolFees { get; internal set; }

		public ulong OutstandingAsset1Amount { get; internal set; }

		public ulong OutstandingAsset2Amount { get; internal set; }

		public ulong OutstandingLiquidityAssetAmount { get; internal set; }

		public long ValidatorAppId { get; internal set; }

		public long AlgoBalance { get; internal set; }

		public long Round { get; internal set; }

		internal Pool(Asset asset1, Asset asset2) {
		
			if (asset1.Id > asset2.Id) {
				Asset1 = asset1;
				Asset2 = asset2;
			} else {
				Asset1 = asset2;
				Asset2 = asset1;
			}
		}

	}

}
