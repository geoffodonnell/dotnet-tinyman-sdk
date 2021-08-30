using Algorand;
using System;
using System.Configuration;
using Tinyman.V1;
using Tinyman.V1.Action;
using Tinyman.V1.Model;

namespace Tinyman.SwapExample {

	class Program {

		static void Main(string[] args) {

			var settings = ConfigurationManager.AppSettings;
			var mnemonic = settings.Get("Account.Mnemonic");

			if (String.IsNullOrWhiteSpace(mnemonic)) {
				throw new Exception("'Account.Mnemonic' key is not set in App.Config.");
			}

			var account = new Account(mnemonic);

			// Initialize the client
			var client = new TinymanTestnetClient();

			// Ensure the account is opted in
			var isOptedIn = client.IsOptedIn(account.Address);

			if (!isOptedIn) {
				client.OptIn(account);
			}

			// Get the assets
			var tinyUsdc = client.FetchAsset(21582668);
			var algo = client.FetchAsset(0);

			// Get the pool
			var pool = client.FetchPool(algo, tinyUsdc);
			var amountIn = Algorand.Utils.AlgosToMicroalgos(1.0);
			var quote = pool.CalculateFixedInputSwapQuote(new AssetAmount(algo, amountIn), 0.05);

			// Check the quote, ensure it's something that you want to execute

			// Convert to action
			var action = Swap.FromQuote(quote);

			// Perform the swap
			try {
				var result = client.Swap(account, action);

				Console.WriteLine($"Swap complete, transaction ID: {result.TxId}");

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
