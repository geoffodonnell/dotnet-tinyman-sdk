namespace Tinyman.V2 {

	public static class Constant {

		public const string PoolLogicSigTemplateAsB64 = "BoAYAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgQBbNQA0ADEYEkQxGYEBEkSBAUM=";

		public const string BootstrapAppArgument = "bootstrap";
		public const string AddLiquidityAppArgument = "add_liquidity";
		public const string AddInitialLiquidityAppArgument = "add_initial_liquidity";
		public const string RemoveLiquidityAppArgument = "remove_liquidity";
		public const string SwapAppArgument = "swap";
		public const string FlashLoanAppArgument = "flash_loan";
		public const string VerifyFlashLoanAppArgument = "verify_flash_loan";
		public const string ClaimFeesAppArgument = "claim_fees";
		public const string ClaimExtraAppArgument = "claim_extra";
		public const string SetFeeAppArgument = "set_fee";
		public const string SetFeeCollectorAppArgument = "set_fee_collector";
		public const string SetFeeSetterAppArgument = "set_fee_setter";
		public const string SetFeeManagerAppArgument = "set_fee_manager";
		
		public const string FixedInputAppArgument = "fixed-input";
		public const string FixedOutputAppArgument = "fixed-output";

		public const string AddLiquidityFlexibleModeAppArgument = "flexible";
		public const string AddLiquiditySingleModeAppArgument = "single";

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
