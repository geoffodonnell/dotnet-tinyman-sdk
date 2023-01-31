using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tinyman.Model;
using Tinyman.V2;

namespace Tinyman.UnitTest.V2 {

	[TestClass]
	public class V2_Pool_Burn_TestCases {

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
		public void Proportional_Burn_TC01() {

			var input = new AssetAmount(AssetLiquidity, 19472); // 0.019472
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(509ul, asset1Amount.Amount); // 0.00509
			Assert.AreEqual(747_768ul, asset2Amount.Amount); // 0.747768
		}

		[TestMethod]
		public void Proportional_Burn_TC02() {

			var input = new AssetAmount(AssetLiquidity, 20121); // 0.020121
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(526ul, asset1Amount.Amount); // 0.00526
			Assert.AreEqual(772_692ul, asset2Amount.Amount); // 0.772692
		}

		[TestMethod]
		public void Proportional_Burn_TC03() {

			var input = new AssetAmount(AssetLiquidity, 20770); // 0.020770
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(543ul, asset1Amount.Amount); // 0.00543
			Assert.AreEqual(797_616ul, asset2Amount.Amount); // 0.797616
		}

		[TestMethod]
		public void Proportional_Burn_TC04() {

			var input = new AssetAmount(AssetLiquidity, 21419); // 0.021419
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(560ul, asset1Amount.Amount); // 0.00560
			Assert.AreEqual(822_541ul, asset2Amount.Amount); // 0.822541
		}

		[TestMethod]
		public void Proportional_Burn_TC05() {

			var input = new AssetAmount(AssetLiquidity, 22068); // 0.022068
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(577ul, asset1Amount.Amount); // 0.00577
			Assert.AreEqual(847_503ul, asset2Amount.Amount); // 0.847503
		}

		[TestMethod]
		public void Proportional_Burn_TC06() {

			var input = new AssetAmount(AssetLiquidity, 22717); // 0.022717
			var result = Pool.CalculateBurnQuote(input, 0.005);

			var asset1Amount = result.AmountsOut.Item1.Asset == Asset1
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			var asset2Amount = result.AmountsOut.Item1.Asset == Asset2
				? result.AmountsOut.Item1 : result.AmountsOut.Item2;

			Assert.AreEqual(594ul, asset1Amount.Amount); // 0.00594
			Assert.AreEqual(872_428ul, asset2Amount.Amount); // 0.872428
		}

		[TestMethod]
		public void Single_Asset_Burn_TC01() {

			var input = new AssetAmount(AssetLiquidity, 19472); // 0.019472
			var result = Pool.CalculateSingleAssetBurnQuote(input, Asset2, 0.005);

			var asset2Amount = result.AmountOut;

			Assert.AreEqual(1_488_968ul, asset2Amount.Amount); // 1.488968
			Assert.AreEqual(0ul, asset2Amount.Asset.Id);
		}

		[TestMethod]
		public void Single_Asset_Burn_TC02() {

			var input = new AssetAmount(AssetLiquidity, 25313); // 0.025313
			var result = Pool.CalculateSingleAssetBurnQuote(input, Asset2, 0.005);

			var asset2Amount = result.AmountOut;

			Assert.AreEqual(1_935_530ul, asset2Amount.Amount); // 1.93553
			Assert.AreEqual(0ul, asset2Amount.Asset.Id);
		}

		[TestMethod]
		public void Single_Asset_Burn_TC03() {

			var input = new AssetAmount(AssetLiquidity, 39593); // 0.039593
			var result = Pool.CalculateSingleAssetBurnQuote(input, Asset2, 0.005);

			var asset2Amount = result.AmountOut;

			Assert.AreEqual(3_019_870ul, asset2Amount.Amount); // 3.01987
			Assert.AreEqual(0ul, asset2Amount.Asset.Id);
		}

		[TestMethod]
		public void Single_Asset_Burn_TC04() {

			var input = new AssetAmount(AssetLiquidity, 48030); // 0.048030
			var result = Pool.CalculateSingleAssetBurnQuote(input, Asset2, 0.005);

			var asset2Amount = result.AmountOut;

			Assert.AreEqual(3_661_291ul, asset2Amount.Amount); // 3.661291
			Assert.AreEqual(0ul, asset2Amount.Asset.Id);
		}

		[TestMethod]
		public void Single_Asset_Burn_TC05() {

			var input = new AssetAmount(AssetLiquidity, 53872); // 0.053872
			var result = Pool.CalculateSingleAssetBurnQuote(input, Asset2, 0.005);

			var asset2Amount = result.AmountOut;

			Assert.AreEqual(4_103_064ul, asset2Amount.Amount); // 4.103064
			Assert.AreEqual(0ul, asset2Amount.Asset.Id);
		}

		[TestMethod]
		public void Single_Asset_Burn_TC06() {

			var input = new AssetAmount(AssetLiquidity, 64906); // 0.064906
			var result = Pool.CalculateSingleAssetBurnQuote(input, Asset2, 0.005);

			var asset2Amount = result.AmountOut;

			Assert.AreEqual(4_935_582ul, asset2Amount.Amount); // 4.935582
			Assert.AreEqual(0ul, asset2Amount.Asset.Id);
		}

	}

}
