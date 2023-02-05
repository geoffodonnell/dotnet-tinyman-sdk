using Algorand.Algod.Model;
using System;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V1;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.IntegrationTestConsole {

	internal static class V1Integration {

		private const double Slippage = 0.001;

		public static async Task PerformSteps(Account account, Asset asset1, Asset asset2) {

			var client = new TinymanV1TestnetClient();

			Console.WriteLine($"Running V1 integration tests...");

			// Opt-in if needed
			var isOptedIn = await client.IsOptedInAsync(account.Address);
			if (!isOptedIn ) {
				Console.WriteLine($"Opting in to Tinyman V1...");
				var appOptInResult = await client.OptInAsync(account);
				Console.WriteLine($"Opt-in complete; tx {appOptInResult.Txid}.");
			}

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
				new AssetAmount(asset2, 500_000_000_000)), Slippage);
			var mintResult1 = await client.MintAsync(account, mintQuote1);
			Console.WriteLine($"Mint complete; tx {mintResult1.Txid}.");

			// 3. More liquidity -- proportional quote
			Console.WriteLine($"Minting additional liquidity (proportional)...");
			pool = await client.FetchPoolAsync(pool.Address);
			var mintQuote2 = pool.CalculateMintQuote(new AssetAmount(asset1, 1_000_000_000), Slippage);
			var mintResult2 = await client.MintAsync(account, mintQuote2);
			Console.WriteLine($"Mint complete; tx {mintResult2.Txid}.");

			// 4. Fixed input swap for asset2
			Console.WriteLine($"Swapping [fixed input] {asset1} <-> {asset2}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote1 = pool.CalculateFixedInputSwapQuote(new AssetAmount(asset1, 10_000), Slippage);
			var swapResult1 = await client.SwapAsync(account, swapQuote1);
			Console.WriteLine($"Swap complete; tx {swapResult1.Txid}.");

			// 5. Fixed input swap for asset1
			Console.WriteLine($"Swapping [fixed input] {asset2} <-> {asset1}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote2 = pool.CalculateFixedInputSwapQuote(new AssetAmount(asset2, 11_000), Slippage);
			var swapResult2 = await client.SwapAsync(account, swapQuote2);
			Console.WriteLine($"Swap complete; tx {swapResult2.Txid}.");

			// 6. Fixed output swap for asset2
			Console.WriteLine($"Swapping [fixed output] {asset1} <-> {asset2}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote3 = pool.CalculateFixedOutputSwapQuote(new AssetAmount(asset2, 13_000), Slippage);
			var swapResult3 = await client.SwapAsync(account, swapQuote3);
			Console.WriteLine($"Swap complete; tx {swapResult3.Txid}.");

			// 7. Fixed output swap for asset1
			Console.WriteLine($"Swapping [fixed output] {asset2} <-> {asset1}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote4 = pool.CalculateFixedOutputSwapQuote(new AssetAmount(asset1, 14_000), Slippage);
			var swapResult4 = await client.SwapAsync(account, swapQuote4);
			Console.WriteLine($"Swap complete; tx {swapResult4.Txid}.");

			// 8. Burn liquidity
			Console.WriteLine($"Burning liquidity...");
			pool = await client.FetchPoolAsync(pool.Address);
			var liquidityAssetBalance3 = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			var burnQuote3 = pool.CalculateBurnQuote(liquidityAssetBalance3 * 0.2, Slippage);
			var burnResult3 = await client.BurnAsync(account, burnQuote3);
			Console.WriteLine($"Burned liquidity; tx {burnResult3.Txid}");

			// 9. Burn remaining liquidity
			Console.WriteLine($"Burning remaining liquidity...");
			pool = await client.FetchPoolAsync(pool.Address);
			var liquidityAssetBalance4 = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			var burnQuote4 = pool.CalculateBurnQuote(liquidityAssetBalance4, Slippage);
			var burnResult4 = await client.BurnAsync(account, burnQuote4);
			Console.WriteLine($"Burned liquidity; tx {burnResult4.Txid}");

			Console.WriteLine($"V1 integration tests complete.");
		}

	}

}
