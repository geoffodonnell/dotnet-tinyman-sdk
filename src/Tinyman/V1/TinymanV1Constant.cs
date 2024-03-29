﻿namespace Tinyman.V1 {

	/// <summary>
	/// Tinyman V1 AMM constant values
	/// </summary>
	public static class TinymanV1Constant {

		public const string AlgodTestnetHost = "https://testnet-api.algonode.cloud";
		public const string AlgodMainnetHost = "https://mainnet-api.algonode.cloud";

		public const ulong TestnetValidatorAppIdV1_0 = 21580889;
		public const ulong TestnetValidatorAppIdV1_1 = 62368684;

		public const ulong MainnetValidatorAppIdV1_0 = 350338509;
		public const ulong MainnetValidatorAppIdV1_1 = 552635992;

		public const ulong TestnetValidatorAppId = TestnetValidatorAppIdV1_1;
		public const ulong MainnetValidatorAppId = MainnetValidatorAppIdV1_1;

		public const ulong BurnFee = 3000;
		public const ulong MintFee = 2000;
		public const ulong RedeemFee = 2000;
		public const ulong RedeemFeesFee = 2000;
		public const ulong SwapFee = 2000;

	}

}
