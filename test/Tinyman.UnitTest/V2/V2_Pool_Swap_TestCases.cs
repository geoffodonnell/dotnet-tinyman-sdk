using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tinyman.Model;
using Tinyman.V2;

namespace Tinyman.UnitTest.V2 {

	[TestClass]
	public class V2_Pool_Swap_TestCases {

		public static readonly ulong AppId = TinymanV2Constant.TestnetValidatorAppIdV2_0;

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

		public static readonly TinymanV2Pool Pool = new TinymanV2Pool {
			Asset1 = Asset1,
			Asset2 = Asset2,
			LiquidityAsset = AssetLiquidity,
			Address = "DNPVHOLSYCBDA6UAB3MREZN6W4MZZV4OL227B5ABABQTHCJFMLD345JPXE",
			AlgoBalance = 148804015,
			Asset1ProtocolFees = 25,
			Asset1Reserves = 100670,
			Asset2ProtocolFees = 42597,
			Asset2Reserves = 147919418,
			Exists = true,
			IssuedLiquidity = 3851649,
			ProtocolFeeRatio = 6,
			Round = 27387103,
			TotalFeeShare = 30,
			ValidatorAppId = AppId
		};

		[TestMethod]
		public void Fixed_Input_Swap_TC01() {

			var input = new AssetAmount(Asset2, 1_560_000); // 1.56
			var result = Pool.CalculateFixedInputSwapQuote(input, 0.005);

			Assert.IsNotNull(result);
			Assert.AreEqual(SwapType.FixedInput, result.SwapType);
			Assert.AreEqual(0.005, result.Slippage);
			Assert.AreEqual(0.01384, result.PriceImpact);
			Assert.AreEqual(1_560_000ul, result.AmountIn.Amount);
			Assert.AreEqual(Asset2, result.AmountIn.Asset);
			Assert.AreEqual(1047ul, result.AmountOut.Amount);
			Assert.AreEqual(Asset1, result.AmountOut.Asset);
			Assert.AreEqual(1042ul, result.AmountOutWithSlippage.Amount);
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
			Assert.AreEqual(0.01421, result.PriceImpact);
			Assert.AreEqual(1_560_000ul, result.AmountOut.Amount);
			Assert.AreEqual(Asset2, result.AmountOut.Asset);
			Assert.AreEqual(1077ul, result.AmountIn.Amount);
			Assert.AreEqual(Asset1, result.AmountIn.Asset);
			Assert.AreEqual(1082ul, result.AmountInWithSlippage.Amount);
			Assert.AreEqual(Asset1, result.AmountInWithSlippage.Asset);
			Assert.AreEqual(3ul, result.SwapFees.Amount);
			Assert.AreEqual(Asset1, result.SwapFees.Asset);
		}

	}

}
