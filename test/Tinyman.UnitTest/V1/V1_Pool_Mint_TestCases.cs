using Algorand.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tinyman.Model;
using Tinyman.V1;

namespace Tinyman.UnitTest.V1 {

	[TestClass]
	public class V1_Pool_Mint_TestCases {

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
		public void Proportional_Mint_TC01() {

			var input = new AssetAmount(Asset2, 500_000); // 0.5
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(349ul, asset1Amount.Amount); // 0.00349
			Assert.AreEqual(500_000ul, asset2Amount.Amount); // 0.5
			Assert.AreEqual(13_220ul, result.LiquidityAssetAmount.Amount); // 0.13220
		}

		[TestMethod]
		public void Proportional_Mint_TC02() {

			var input = new AssetAmount(Asset2, 500_010); // 0.50001
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(349ul, asset1Amount.Amount); // 0.00349
			Assert.AreEqual(500_010ul, asset2Amount.Amount); // 0.50001
			Assert.AreEqual(13_220ul, result.LiquidityAssetAmount.Amount); // 0.13220
		}

		[TestMethod]
		public void Proportional_Mint_TC03() {

			var input = new AssetAmount(Asset2, 500_100); // 0.5001
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(349ul, asset1Amount.Amount); // 0.00349
			Assert.AreEqual(500_100ul, asset2Amount.Amount); // 0.5001
			Assert.AreEqual(13_220ul, result.LiquidityAssetAmount.Amount); // 0.13220
		}

		[TestMethod]
		public void Proportional_Mint_TC04() {

			var input = new AssetAmount(Asset2, 501_000); // 0.501
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(349ul, asset1Amount.Amount); // 0.00349
			Assert.AreEqual(501_000ul, asset2Amount.Amount); // 0.501
			Assert.AreEqual(13_220ul, result.LiquidityAssetAmount.Amount); // 0.13220
		}

		[TestMethod]
		public void Proportional_Mint_TC05() {

			var input = new AssetAmount(Asset2, 510_000); // 0.51
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(356ul, asset1Amount.Amount); // 0.00356
			Assert.AreEqual(510_000ul, asset2Amount.Amount); // 0.51
			Assert.AreEqual(13_485ul, result.LiquidityAssetAmount.Amount); // 0.13485
		}

	}

}
