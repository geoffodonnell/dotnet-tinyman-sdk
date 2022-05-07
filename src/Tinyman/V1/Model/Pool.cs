namespace Tinyman.V1.Model {

	public class Pool {

		public virtual bool Exists { get; set; }

		public virtual string Address { get; set; }

		public virtual Asset Asset1 { get; set; }

		public virtual Asset Asset2 { get; set; }

		public virtual Asset LiquidityAsset { get; set; }

		public virtual ulong Asset1Reserves { get; set;  }

		public virtual ulong Asset2Reserves { get; set; }

		public virtual ulong IssuedLiquidity { get; set; }

		public virtual ulong UnclaimedProtocolFees { get; set; }

		public virtual ulong OutstandingAsset1Amount { get; set; }

		public virtual ulong OutstandingAsset2Amount { get; set; }

		public virtual ulong OutstandingLiquidityAssetAmount { get; set; }

		public virtual ulong ValidatorAppId { get; set; }

		public virtual ulong AlgoBalance { get; set; }

		public virtual ulong Round { get; set; }

		public virtual ulong Asset1Price { get => Asset2Reserves / Asset1Reserves; }

		public virtual ulong Asset2Price { get => Asset1Reserves / Asset2Reserves; }

		public Pool() { }

		internal Pool(Asset asset1, Asset asset2) {
		
			if (asset1.Id > asset2.Id) {
				Asset1 = asset1;
				Asset2 = asset2;
			} else {
				Asset1 = asset2;
				Asset2 = asset1;
			}

			Exists = false;
		}

		public virtual AssetAmount Convert(AssetAmount amount) {

			if (amount.Asset == Asset1) {
				return new AssetAmount(Asset2, amount.Amount * Asset1Price);
			}

			if (amount.Asset == Asset2) {
				return new AssetAmount(Asset1, amount.Amount * Asset2Price);
			}

			return null;
		}

	}

}
