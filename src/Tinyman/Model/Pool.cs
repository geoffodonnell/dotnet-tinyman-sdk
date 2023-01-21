namespace Tinyman.Model {

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

		public virtual ulong Asset1Price { get => Asset2Reserves / Asset1Reserves; }

		public virtual ulong Asset2Price { get => Asset1Reserves / Asset2Reserves; }

	}

}