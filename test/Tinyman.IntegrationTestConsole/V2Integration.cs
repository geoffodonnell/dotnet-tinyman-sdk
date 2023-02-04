using Algorand.Algod.Model;
using System;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V2;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.IntegrationTestConsole {

	internal static class V2Integration {

		public static async Task PerformSteps(Account account, Asset asset1, Asset asset2) {

			var client = new TinymanV2TestnetClient();

			Console.WriteLine($"Running V2 integration tests...");

			// 1. Bootstrap pool
			Console.WriteLine($"Bootstrapping {asset1} <-> {asset2} pool...");
			var bootstrapResult = await client.BootstrapAsync(account, asset1, asset2);
			Console.WriteLine($"Bootstrap complete; tx {bootstrapResult.Txid}.");

			Console.WriteLine($"Fetching pool info...");
			var pool = await client.FetchPoolAsync(asset1, asset2);
			Console.WriteLine($"Fetched pool info.");

			Console.WriteLine($"Opting in to pool liquidity asset...");
			var optInResult = await client.OptInToAssetAsync(account, pool.LiquidityAsset);
			Console.WriteLine($"Opted in to pool liquidity asset.");

			// 2. Provide initial liquidity
			Console.WriteLine($"Minting initial liquidity...");
			var mintQuote1 = pool.CalculateMintQuote(new Tuple<AssetAmount, AssetAmount>(
				new AssetAmount(asset1, 1_000_000_000_000),
				new AssetAmount(asset2, 500_000_000_000)));
			var mintResult1 = await client.MintAsync(account, mintQuote1);
			Console.WriteLine($"Mint complete; tx {mintResult1.Txid}.");

			// 3. More liquidity -- proportional quote
			Console.WriteLine($"Minting additional liquidity (proportional)...");
			pool = await client.FetchPoolAsync(pool.Address);
			var mintQuote2 = pool.CalculateMintQuote(new AssetAmount(asset1, 1_000_000_000));
			var mintResult2 = await client.MintAsync(account, mintQuote2);
			Console.WriteLine($"Mint complete; tx {mintResult2.Txid}.");

			// 4. More liquidity -- flexible quote
			Console.WriteLine($"Minting additional liquidity (flexible)...");
			pool = await client.FetchPoolAsync(pool.Address);
			var mintQuote3 = pool.CalculateFlexibleMintQuote(new Tuple<AssetAmount, AssetAmount>(
				new AssetAmount(asset1, 50_000_000_000),
				new AssetAmount(asset2, 50_000_000_000)));
			var mintResult3 = await client.MintAsync(account, mintQuote3);
			Console.WriteLine($"Mint complete; tx {mintResult3.Txid}.");

			// 5. More liquidity -- single asset
			Console.WriteLine($"Minting additional liquidity (single asset)...");
			pool = await client.FetchPoolAsync(pool.Address);
			var mintQuote4 = pool.CalculateSingleAssetMintQuote(
				new AssetAmount(asset2, 50_000_000_000));
			var mintResult4 = await client.MintAsync(account, mintQuote4);
			Console.WriteLine($"Mint complete; tx {mintResult4.Txid}.");

			// 6. Fixed input swap for asset2
			Console.WriteLine($"Swapping [fixed input] {asset1} <-> {asset2}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote1 = pool.CalculateFixedInputSwapQuote(new AssetAmount(asset1, 10_000), 0.00);
			var swapResult1 = await client.SwapAsync(account, swapQuote1);
			Console.WriteLine($"Swap complete; tx {swapResult1.Txid}.");

			// 7. Fixed input swap for asset1
			Console.WriteLine($"Swapping [fixed input] {asset2} <-> {asset1}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote2 = pool.CalculateFixedInputSwapQuote(new AssetAmount(asset2, 11_000), 0.00);
			var swapResult2 = await client.SwapAsync(account, swapQuote2);
			Console.WriteLine($"Swap complete; tx {swapResult2.Txid}.");

			// 8. Fixed output swap for asset2
			Console.WriteLine($"Swapping [fixed output] {asset1} <-> {asset2}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote3 = pool.CalculateFixedOutputSwapQuote(new AssetAmount(asset2, 13_000), 0.00);
			var swapResult3 = await client.SwapAsync(account, swapQuote3);
			Console.WriteLine($"Swap complete; tx {swapResult3.Txid}.");

			// 9. Fixed output swap for asset1
			Console.WriteLine($"Swapping [fixed output] {asset2} <-> {asset1}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote4 = pool.CalculateFixedOutputSwapQuote(new AssetAmount(asset1, 14_000), 0.00);
			var swapResult4 = await client.SwapAsync(account, swapQuote4);
			Console.WriteLine($"Swap complete; tx {swapResult4.Txid}.");

			// 10. Burn liquidity for asset1
			Console.WriteLine($"Burning liquidity for {asset1}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var liquidityAssetBalance1 = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			var burnQuote1 = pool.CalculateSingleAssetBurnQuote(liquidityAssetBalance1 * 0.2, asset1, 0.00);
			var burnResult1 = await client.BurnAsync(account, burnQuote1);
			Console.WriteLine($"Burned liquidity; tx {burnResult1.Txid}");

			// 11. Burn liquidity for asset2
			Console.WriteLine($"Burning liquidity for {asset2}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var liquidityAssetBalance2 = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			var burnQuote2 = pool.CalculateSingleAssetBurnQuote(liquidityAssetBalance2 * 0.2, asset2, 0.00);
			var burnResult2 = await client.BurnAsync(account, burnQuote2);
			Console.WriteLine($"Burned liquidity; tx {burnResult2.Txid}");

			// 11. Burn liquidity
			Console.WriteLine($"Burning liquidity...");
			pool = await client.FetchPoolAsync(pool.Address);
			var liquidityAssetBalance3 = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			var burnQuote3 = pool.CalculateBurnQuote(liquidityAssetBalance3 * 0.2, 0.00);
			var burnResult3 = await client.BurnAsync(account, burnQuote3);
			Console.WriteLine($"Burned liquidity; tx {burnResult3.Txid}");

			// 12. Burn remaining liquidity
			Console.WriteLine($"Burning remaining liquidity...");
			pool = await client.FetchPoolAsync(pool.Address);
			var liquidityAssetBalance4 = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			var burnQuote4 = pool.CalculateBurnQuote(liquidityAssetBalance4, 0.00);
			var burnResult4 = await client.BurnAsync(account, burnQuote4);
			Console.WriteLine($"Burned liquidity; tx {burnResult4.Txid}");

			Console.WriteLine($"V2 integration tests complete.");
		}

	}

}
