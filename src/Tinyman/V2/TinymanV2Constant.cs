using Algorand.Common;

namespace Tinyman.V2 {

	public static class TinymanV2Constant {

		public const string AlgodTestnetHost = "https://testnet-api.algonode.cloud";
		public const string AlgodMainnetHost = "https://mainnet-api.algonode.cloud";

		public const ulong DefaultMinFee = 1000;

		public const string PoolLogicSigTemplateAsB64 = "BoAYAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgQBbNQA0ADEYEkQxGYEBEkSBAUM=";

		// I'm not a real fan of the way these constants are initailized, would be interested in suggestions.
		// Static constructor? Something cooler?

		public readonly static byte[] BootstrapAppArgument = ApplicationArgument.String("bootstrap");
		public readonly static byte[] AddLiquidityAppArgument = ApplicationArgument.String("add_liquidity");
		public readonly static byte[] AddInitialLiquidityAppArgument = ApplicationArgument.String("add_initial_liquidity");
		public readonly static byte[] RemoveLiquidityAppArgument = ApplicationArgument.String("remove_liquidity");
		public readonly static byte[] SwapAppArgument = ApplicationArgument.String("swap");
		public readonly static byte[] FlashLoanAppArgument = ApplicationArgument.String("flash_loan");
		public readonly static byte[] VerifyFlashLoanAppArgument = ApplicationArgument.String("verify_flash_loan");
		public readonly static byte[] ClaimFeesAppArgument = ApplicationArgument.String("claim_fees");
		public readonly static byte[] ClaimExtraAppArgument = ApplicationArgument.String("claim_extra");
		public readonly static byte[] SetFeeAppArgument = ApplicationArgument.String("set_fee");
		public readonly static byte[] SetFeeCollectorAppArgument = ApplicationArgument.String("set_fee_collector");
		public readonly static byte[] SetFeeSetterAppArgument = ApplicationArgument.String("set_fee_setter");
		public readonly static byte[] SetFeeManagerAppArgument = ApplicationArgument.String("set_fee_manager");
		
		public readonly static byte[] FixedInputAppArgument = ApplicationArgument.String("fixed-input");
		public readonly static byte[] FixedOutputAppArgument = ApplicationArgument.String("fixed-output");

		public readonly static byte[] AddLiquidityFlexibleModeAppArgument = ApplicationArgument.String("flexible");
		public readonly static byte[] AddLiquiditySingleModeAppArgument = ApplicationArgument.String("single");

		public const ulong TestnetValidatorAppIdV2_0 = 148607000;
		public const ulong MainnetValidatorAppIdV2_0 = 1002541853;

		public const ulong LockedPoolTokens = 1_000;
		public const ulong AssetMinTotal = 1_000_000;

		public const ulong AppLocalInts = 12;
		public const ulong AppLocalBytes = 2;
		public const ulong AppGlobalInts = 0;
		public const ulong AppGlobalBytes = 3;

		public static readonly ulong MinPoolBalanceAsaAlgoPair = 300_000 + (100_000 + (25_000 + 3_500) * AppLocalInts + (25_000 + 25_000) * AppLocalBytes);
		public static readonly ulong MinPoolBalanceAsaAsaPair = MinPoolBalanceAsaAlgoPair + 100_000;

	}

}
