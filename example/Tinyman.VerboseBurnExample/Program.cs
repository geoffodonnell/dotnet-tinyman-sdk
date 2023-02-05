using Algorand;
using Algorand.Algod.Model;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Tinyman.V2;

namespace Tinyman.VerboseBurnExample {

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

			// Get a quote to swap the entire liquidity pool asset balance for pooled assets
			var amount = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			var quote = pool.CalculateBurnQuote(amount, 0.005);

			// Check the quote, ensure it's something that you want to execute
			
			// Perform the burning
			try {

				var txParams = await client.FetchTransactionParamsAsync();

				// Use utility method to create the transaction group
				var burnTxGroup = TinymanV2Transaction.PrepareBurnTransactions(
						appId,
						quote.AmountsOutWithSlippage.Item1,
						quote.AmountsOutWithSlippage.Item2,
						quote.LiquidityAssetAmount,
						account.Address,
						txParams);

				// Sign the transactions sent from the account,
				// the LogicSig transactions will already be signed
				for (var i = 0; i < burnTxGroup.Transactions.Length; i++) {
					var tx = burnTxGroup.Transactions[i];

					if (tx.Sender.Equals(account.Address)) {

						// Inspect transaction

						// Sign transaction
						burnTxGroup.SignedTransactions[i] = tx.Sign(account);
					}
				}

				var burnResult = await client.SubmitAsync(burnTxGroup);

				Console.WriteLine($"Burn complete, transaction ID: {burnResult.Txid}");

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
