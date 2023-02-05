﻿using Algorand.Algod.Model;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V2;

namespace Tinyman.MintExample {

	class Program {

		static async Task Main(string[] args) {

			var settings = ConfigurationManager.AppSettings;
			var mnemonic = settings.Get("Account.Mnemonic")?.Replace(",", "");

			if (String.IsNullOrWhiteSpace(mnemonic)) {
				throw new Exception("'Account.Mnemonic' key is not set in App.Config.");
			}

			var account = new Account(mnemonic);

			// Initialize the client
			var client = new TinymanV2TestnetClient();

			// Get the assets
			var tinyUsdc = await client.FetchAssetAsync(21582668);
			var algo = await client.FetchAssetAsync(0);

			// Get the pool
			var pool = await client.FetchPoolAsync(algo, tinyUsdc);

			// Get a quote to add 1 Algo and the corresponding tinyUsdc amount to the pool
			var amountIn = Algorand.Utils.Utils.AlgosToMicroalgos(1.0);
			var quote = pool.CalculateMintQuote(new AssetAmount(algo, amountIn), 0.005);

			// Check the quote, ensure it's something that you want to execute
			//

			// Perform the minting
			try {
				var result = await client.MintAsync(account, quote);

				Console.WriteLine($"Mint complete, transaction ID: {result.Txid}");

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
