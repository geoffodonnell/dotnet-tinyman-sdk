using Algorand;
using System;
using System.Configuration;
using Tinyman.V1;
using Tinyman.V1.Model;

namespace Tinyman.VerboseSwapExample {

	class Program {

		static void Main(string[] args) {

			var settings = ConfigurationManager.AppSettings;
			var mnemonic = settings.Get("Account.Mnemonic");

			if (String.IsNullOrWhiteSpace(mnemonic)) {
				throw new Exception("'Account.Mnemonic' key is not set in App.Config.");
			}

			var account = new Account(mnemonic);

			// Initialize the client
			var appId = Constant.TestnetValidatorAppId;
			var algodApi = new Algorand.V2.AlgodApi(
				Constant.AlgodTestnetHost, String.Empty);
			var client = new TinymanClient(algodApi, appId);

			// Ensure the account is opted in
			var isOptedIn = client.IsOptedIn(account.Address);

			if (!isOptedIn) {

				var txParams = algodApi.TransactionParams();

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

				var optInResult = client.Submit(optInTxGroup);

				Console.WriteLine($"Opt-in successful, transaction ID: {optInResult.TxId}");
			}

			// Get the assets
			var tinyUsdc = client.FetchAsset(21582668);
			var algo = client.FetchAsset(0);

			// Get the pool
			var pool = client.FetchPool(algo, tinyUsdc);

			// Get a quote to swap 1 Algo for tinyUsdc
			var amountIn = Algorand.Utils.AlgosToMicroalgos(1.0);
			var quote = pool.CalculateFixedInputSwapQuote(new AssetAmount(algo, amountIn), 0.05);

			// Check the quote, ensure it's something that you want to execute

			// Perform the swap
			try {

				var txParams = algodApi.TransactionParams();

				// Use utility method to create the transaction group
				var swapTxGroup = TinymanTransaction.PrepareSwapTransactions(
						appId,
						quote.AmountIn,
						quote.AmountOutWithSlippage,
						pool.LiquidityAsset,
						quote.SwapType,
						account.Address,
						txParams);

				// Sign the transactions sent from the account,
				// the LogicSig transactions will already be signed
				for (var i = 0; i < swapTxGroup.Transactions.Length; i++) {
					var tx = swapTxGroup.Transactions[i];

					// Inspect transaction

					// Sign transaction
					if (tx.sender.Equals(account.Address)) {
						swapTxGroup.SignedTransactions[i] = account.SignTransaction(tx);
					}
				}

				var swapResult = client.Submit(swapTxGroup);

				Console.WriteLine($"Swap complete, transaction ID: {swapResult.TxId}");

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
