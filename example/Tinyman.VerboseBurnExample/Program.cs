using Algorand;
using Algorand.V2;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Tinyman.V1;
using Tinyman.V1.Model;

namespace Tinyman.VerboseBurnExample {

	class Program {

		static async Task Main(string[] args) {

			var settings = ConfigurationManager.AppSettings;
			var mnemonic = settings.Get("Account.Mnemonic");

			if (String.IsNullOrWhiteSpace(mnemonic)) {
				throw new Exception("'Account.Mnemonic' key is not set in App.Config.");
			}

			var account = new Account(mnemonic);

			// Initialize the client
			var appId = Constant.TestnetValidatorAppId;
			var url = Constant.AlgodTestnetHost;
			var token = String.Empty;
			var httpClient = HttpClientConfigurator.ConfigureHttpClient(url, token);
			var client = new TinymanClient(httpClient, url, appId);

			// Ensure the account is opted in
			var isOptedIn = await client.IsOptedInAsync(account.Address);

			if (!isOptedIn) {

				var txParams = await client.FetchTransactionParamsAsync();

				// Use utility method to create the transaction group
				var optInTxGroup = TinymanTransaction
					.PrepareAppOptinTransactions(appId, account.Address, txParams);

				// Sign the transactions sent from the account,
				// the LogicSig transactions will already be signed
				for (var i = 0; i < optInTxGroup.Transactions.Length; i++) {
					var tx = optInTxGroup.Transactions[i];

					// Inspect transaction

					// Sign transaction
					if (tx.sender.Equals(account.Address)) {
						optInTxGroup.SignedTransactions[i] = account.SignTransaction(tx);
					}
				}

				var optInResult = await client.SubmitAsync(optInTxGroup);

				Console.WriteLine($"Opt-in successful, transaction ID: {optInResult.TxId}");
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
			
			// Perform the burning
			try {

				var txParams = await client.FetchTransactionParamsAsync();

				// Use utility method to create the transaction group
				var burnTxGroup = TinymanTransaction.PrepareBurnTransactions(
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

					if (tx.sender.Equals(account.Address)) {

						// Inspect transaction

						// Sign transaction
						burnTxGroup.SignedTransactions[i] = account.SignTransaction(tx);
					}
				}

				var burnResult = await client.SubmitAsync(burnTxGroup);

				Console.WriteLine($"Burn complete, transaction ID: {burnResult.TxId}");

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
