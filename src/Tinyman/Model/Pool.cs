namespace Tinyman.Model {

	/// <summary>
	/// Common properties and methods of Tinyman asset pools
	/// </summary>
	public class Pool {

		public virtual bool Exists { get; set; }

		public virtual string Address { get; set; }

		public virtual Asset Asset1 { get; set; }

		public virtual Asset Asset2 { get; set; }

		public virtual Asset LiquidityAsset { get; set; }

		public virtual ulong Asset1Reserves { get; set; }

		public virtual ulong Asset2Reserves { get; set; }

		public virtual ulong IssuedLiquidity { get; set; }

		public virtual ulong ValidatorAppId { get; set; }

		public virtual ulong AlgoBalance { get; set; }

		public virtual ulong Round { get; set; }

		public virtual decimal Asset1Price { get => (decimal)Asset2Reserves / (decimal)Asset1Reserves; }

		public virtual decimal Asset2Price { get => (decimal)Asset1Reserves / (decimal)Asset2Reserves; }

		public virtual AssetAmount Convert(AssetAmount amount) {

			if (amount.Asset == Asset1) {
				return new AssetAmount(Asset2, (ulong)(amount.Amount * Asset1Price));
			}

			if (amount.Asset == Asset2) {
				return new AssetAmount(Asset1, (ulong)(amount.Amount * Asset2Price));
			}

			return null;
		}

	}

}