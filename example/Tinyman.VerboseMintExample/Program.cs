using Algorand;
using Algorand.Algod.Model;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V2;

namespace Tinyman.VerboseMintExample {

	class Program {

		static async Task Main(string[] args) {

			var settings = ConfigurationManager.AppSettings;
			var mnemonic = settings.Get("Account.Mnemonic")?.Replace(",", "");

			if (String.IsNullOrWhiteSpace(mnemonic)) {
				throw new Exception("'Account.Mnemonic' key is not set in App.Config.");
			}

			var account = new Account(mnemonic);

			// Initialize the client
			var appId = TinymanV2Constant.TestnetValidatorAppIdV2_0;
			var url = TinymanV2Constant.AlgodTestnetHost;
			var token = String.Empty;
			var httpClient = HttpClientConfigurator.ConfigureHttpClient(url, token);
			var client = new TinymanV2Client(httpClient, url, appId);

			// Get the assets
			var tinyUsdc = await client.FetchAssetAsync(21582668);
			var algo = await client.FetchAssetAsync(0);

			// Get the pool
			var pool = await client.FetchPoolAsync(algo, tinyUsdc);

			// Get a quote to add 1 Algo and the corresponding tinyUsdc amount to the pool
			var amountIn = Algorand.Utils.Utils.AlgosToMicroalgos(1.0);
			var quote = pool.CalculateMintQuote(new AssetAmount(algo, amountIn), 0.005);

			// Check the quote, ensure it's something that you want to execute

			// Perform the swap
			try {

				var txParams = await client.FetchTransactionParamsAsync();

				// Use utility method to create the transaction group
				var mintTxGroup = TinymanV2Transaction.PrepareMintTransactions(
						appId,
						quote.AmountsIn.Item1,
						quote.AmountsIn.Item2,
						quote.LiquidityAssetAmountWithSlippage,
						account.Address,
						txParams);

				// Sign the transactions sent from the account,
				// the LogicSig transactions will already be signed
				for (var i = 0; i < mintTxGroup.Transactions.Length; i++) {
					var tx = mintTxGroup.Transactions[i];

					// Inspect transaction

					// Sign transaction
					if (tx.Sender.Equals(account.Address)) {
						mintTxGroup.SignedTransactions[i] = tx.Sign(account);
					}
				}

				var mintResult = await client.SubmitAsync(mintTxGroup);

				Console.WriteLine($"Swap complete, transaction ID: {mintResult.Txid}");

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
