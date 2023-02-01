using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tinyman.Model;
using Tinyman.V2;

namespace Tinyman.UnitTest.V2 {

	[TestClass]
	public class V2_Pool_Mint_TestCases {

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
		public void Proportional_Mint_TC01() {

			var input = new AssetAmount(Asset2, 500_000); // 0.5
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(340ul, asset1Amount.Amount); // 0.00340
			Assert.AreEqual(500_000ul, asset2Amount.Amount); // 0.5
			Assert.AreEqual(13_013ul, result.LiquidityAssetAmount.Amount); // 0.13013
		}

		[TestMethod]
		public void Proportional_Mint_TC02() {

			var input = new AssetAmount(Asset2, 500_010); // 0.50001
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(340ul, asset1Amount.Amount); // 0.00340
			Assert.AreEqual(500_010ul, asset2Amount.Amount); // 0.50001
			Assert.AreEqual(13_014ul, result.LiquidityAssetAmount.Amount); // 0.13014
		}

		[TestMethod]
		public void Proportional_Mint_TC03() {

			var input = new AssetAmount(Asset2, 500_100); // 0.5001
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(340ul, asset1Amount.Amount); // 0.00340
			Assert.AreEqual(500_100ul, asset2Amount.Amount); // 0.5001
			Assert.AreEqual(13_015ul, result.LiquidityAssetAmount.Amount); // 0.13015
		}

		[TestMethod]
		public void Proportional_Mint_TC04() {

			var input = new AssetAmount(Asset2, 501_000); // 0.501
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(340ul, asset1Amount.Amount); // 0.00340
			Assert.AreEqual(501_000ul, asset2Amount.Amount); // 0.501
			Assert.AreEqual(13_026ul, result.LiquidityAssetAmount.Amount); // 0.13026
		}

		[TestMethod]
		public void Proportional_Mint_TC05() {

			var input = new AssetAmount(Asset2, 510_000); // 0.51
			var result = Pool.CalculateMintQuote(input, 0.005);

			var asset1Amount = result.AmountsIn.Item1.Asset == Asset1
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			var asset2Amount = result.AmountsIn.Item1.Asset == Asset2
				? result.AmountsIn.Item1 : result.AmountsIn.Item2;

			Assert.AreEqual(347ul, asset1Amount.Amount); // 0.00347
			Assert.AreEqual(510_000ul, asset2Amount.Amount); // 0.51
			Assert.AreEqual(13_278ul, result.LiquidityAssetAmount.Amount); // 0.13278
		}

	}

}
