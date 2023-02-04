using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tinyman.Model;
using Tinyman.V1;

namespace Tinyman.UnitTest.V1 {

	[TestClass]
	public class V1_Pool_Swap_TestCases {

		public static readonly ulong AppId = TinymanV1Constant.TestnetValidatorAppIdV1_1;

		public static readonly Asset Asset1 = new Asset {
			Id = 21582981,
			Name = "Tiny Gold",
			UnitName = "TINYAU",
			Decimals = 5
		};

		public static readonly Asset Asset2 = new Asset {
			Id = 0,
			Name = "Algo",
			UnitName = "ALGO",
			Decimals = 6
		};

		public static readonly Asset AssetLiquidity = new Asset {
			Id = 152331250,
			Name = "TinymanPool2.0 TINYAU-ALGO",
			UnitName = "TMPOOL2",
			Decimals = 6
		};

		public static readonly TinymanV1Pool Pool = new TinymanV1Pool {
			Asset1 = Asset1,
			Asset2 = Asset2,
			LiquidityAsset = AssetLiquidity,
			Address = "4CKCFP64MAECN3QYF4F6O2B6M2X66YEKGRJ2OGXMXS53R4LHAPTY5JAIGY",
			AlgoBalance = 54397369,
			Asset1Reserves = 37398,
			OutstandingAsset1Amount = 140,
			Asset2Reserves = 53540620,
			OutstandingAsset2Amount = 749,
			Exists = true,
			IssuedLiquidity = 1416684,
			OutstandingLiquidityAssetAmount = 5730,
			Round = 27387103,
			UnclaimedProtocolFees = 4730,
			ValidatorAppId = AppId
		};

		[TestMethod]
		public void Fixed_Input_Swap_TC01() {

			var input = new AssetAmount(Asset2, 1_560_000); // 1.56
			var result = Pool.CalculateFixedInputSwapQuote(input, 0.005);

			Assert.IsNotNull(result);
			Assert.AreEqual(SwapType.FixedInput, result.SwapType);
			Assert.AreEqual(0.005, result.Slippage);
			Assert.IsNull(result.PriceImpact);
			Assert.AreEqual(1_560_000ul, result.AmountIn.Amount);
			Assert.AreEqual(Asset2, result.AmountIn.Asset);
			Assert.AreEqual(1056ul, result.AmountOut.Amount);
			Assert.AreEqual(Asset1, result.AmountOut.Asset);
			Assert.AreEqual(1051ul, result.AmountOutWithSlippage.Amount);
			Assert.AreEqual(Asset1, result.AmountOutWithSlippage.Asset);
			Assert.AreEqual(4680ul, result.SwapFees.Amount);
			Assert.AreEqual(Asset2, result.SwapFees.Asset);
		}

		[TestMethod]
		public void Fixed_Output_Swap_TC01() {

			var input = new AssetAmount(Asset2, 1_560_000); // 1.56
			var result = Pool.CalculateFixedOutputSwapQuote(input, 0.005);

			Assert.IsNotNull(result);
			Assert.AreEqual(SwapType.FixedOutput, result.SwapType);
			Assert.AreEqual(0.005, result.Slippage);
			Assert.IsNull(result.PriceImpact);
			Assert.AreEqual(1_560_000ul, result.AmountOut.Amount);
			Assert.AreEqual(Asset2, result.AmountOut.Asset);
			Assert.AreEqual(1125ul, result.AmountIn.Amount);
			Assert.AreEqual(Asset1, result.AmountIn.Asset);
			Assert.AreEqual(1131ul, result.AmountInWithSlippage.Amount);
			Assert.AreEqual(Asset1, result.AmountInWithSlippage.Asset);
			Assert.AreEqual(3ul, result.SwapFees.Amount);
			Assert.AreEqual(Asset1, result.SwapFees.Asset);
		}

	}

}
