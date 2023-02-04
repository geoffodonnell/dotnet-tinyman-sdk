using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tinyman.Model;
using Tinyman.V1;

namespace Tinyman.UnitTest.V1 {

	[TestClass]
	public class V1_Pool_Burn_TestCases {

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
		public void Proportional_Burn_TC01() {

			var input = new AssetAmount(AssetLiquidity, 19472); // 0.019472
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(514ul, asset1Amount.Amount); // 0.00514
			Assert.AreEqual(735_903ul, asset2Amount.Amount); // 0.735903
		}

		[TestMethod]
		public void Proportional_Burn_TC02() {

			var input = new AssetAmount(AssetLiquidity, 20121); // 0.020121
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(531ul, asset1Amount.Amount); // 0.00531
			Assert.AreEqual(760_431ul, asset2Amount.Amount); // 0.760431
		}

		[TestMethod]
		public void Proportional_Burn_TC03() {

			var input = new AssetAmount(AssetLiquidity, 20770); // 0.020770
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(548ul, asset1Amount.Amount); // 0.00548
			Assert.AreEqual(784_958ul, asset2Amount.Amount); // 0.784958
		}

		[TestMethod]
		public void Proportional_Burn_TC04() {

			var input = new AssetAmount(AssetLiquidity, 21419); // 0.021419
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(565ul, asset1Amount.Amount); // 0.00565
			Assert.AreEqual(809_486ul, asset2Amount.Amount); // 0.809486
		}

		[TestMethod]
		public void Proportional_Burn_TC05() {

			var input = new AssetAmount(AssetLiquidity, 22068); // 0.022068
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(582ul, asset1Amount.Amount); // 0.00582
			Assert.AreEqual(834_014ul, asset2Amount.Amount); // 0.834014
		}

		[TestMethod]
		public void Proportional_Burn_TC06() {

			var input = new AssetAmount(AssetLiquidity, 22717); // 0.022717
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(599ul, asset1Amount.Amount); // 0.00599
			Assert.AreEqual(858_541ul, asset2Amount.Amount); // 0.858541
		}

	}

}
