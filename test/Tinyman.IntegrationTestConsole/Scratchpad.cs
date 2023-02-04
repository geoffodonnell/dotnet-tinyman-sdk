using Algorand.Algod.Model;
using System;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V1;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.IntegrationTestConsole {
	internal static class Scratchpad {

		public static async Task PerformSteps(Account account) {

			var client = new TinymanV1TestnetClient();

			var asset1 = await client.FetchAssetAsync(156884654);
			var asset2 = await client.FetchAssetAsync(156884655);
			var pool = await client.FetchPoolAsync(asset1, asset2);

			// 4. Fixed input swap for asset2
			Console.WriteLine($"Swapping [fixed input] {asset1} <-> {asset2}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote1 = pool.CalculateFixedInputSwapQuote(new AssetAmount(asset1, 10_000), 0.00);
			var swapResult1 = await client.SwapAsync(account, swapQuote1);
			Console.WriteLine($"Swap complete; tx {swapResult1.Txid}.");
		}
	}
}
