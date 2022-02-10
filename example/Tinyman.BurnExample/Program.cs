using Algorand;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Tinyman.V1;
using Tinyman.V1.Action;
using Tinyman.V1.Model;

namespace Tinyman.BurnExample {

	class Program {

		static async Task Main(string[] args) {

			var settings = ConfigurationManager.AppSettings;
			var mnemonic = settings.Get("Account.Mnemonic")?.Replace(",", "");

			if (String.IsNullOrWhiteSpace(mnemonic)) {
				throw new Exception("'Account.Mnemonic' key is not set in App.Config.");
			}

			var account = new Account(mnemonic);

			// Initialize the client
			var client = new TinymanTestnetClient();

			// Ensure the account is opted in
			var isOptedIn = await client.IsOptedInAsync(account.Address);

			if (!isOptedIn) {
				await client.OptInAsync(account);
			}

			// Get the assets
			var tinyUsdc = await client.FetchAssetAsync(21582668);
			var algo = await client.FetchAssetAsync(0);

			// Get the pool
			var pool = await client.FetchPoolAsync(algo, tinyUsdc);

			// Get a quote to swap the entire liquidity pool asset balance for pooled assets
			var amount = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			var quote = pool.CalculateBurnQuote(amount, 0.05);

			// Check the quote, ensure it's something that you want to execute

			// Convert to action
			var action = Burn.FromQuote(quote);

			// Perform the burning
			try {
				var result = await client.BurnAsync(account, action);

				Console.WriteLine($"Burn complete, transaction ID: {result.TxId}");

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
