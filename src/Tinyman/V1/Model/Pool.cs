using System;
using System.Collections.Generic;
using System.Text;

namespace Tinyman.V1.Model {

	public class Pool {

		public string Address { get; internal set; }

		public long Asset1Id { get; internal set; }

		public long Asset2Id { get; internal set; }

		public long LiquidityAssetId { get; internal set; }

		public string LiquidityAssetName { get; internal set; }

		public long Asset1Reserves { get; internal set;  }

		public long Asset2Reserves { get; internal set; }

		public long IssuedLiquidity { get; internal set; }

		public long UnclaimedProtocolFees { get; internal set; }

		public long OutstandingAsset1Amount { get; internal set; }

		public long OutstandingAsset2Amount { get; internal set; }

		public long OutstandingLiquidityAssetAmount { get; internal set; }

		public long ValidatorAppId { get; internal set; }

		public long AlgoBalance { get; internal set; }

		public long Round { get; internal set; }

	}

}
