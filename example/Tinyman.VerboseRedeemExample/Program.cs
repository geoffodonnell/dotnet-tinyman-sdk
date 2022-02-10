using Algorand;
using Algorand.V2;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Tinyman.V1;
using Tinyman.V1.Model;

namespace Tinyman.VerboseRedeemExample {

	class Program {

		static async Task Main(string[] args) {

			var settings = ConfigurationManager.AppSettings;
			var mnemonic = settings.Get("Account.Mnemonic")?.Replace(",", "");

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

			// Fetch the amounts
			var excessAmounts = await client.FetchExcessAmountsAsync(account.Address);

			if (excessAmounts == null || excessAmounts.Count == 0) {
				Console.WriteLine("No excess amounts to redeem");
				return;
			}

			try {

				// Redeem each amount
				foreach (var quote in excessAmounts) {

					var txParams = await client.FetchTransactionParamsAsync();
					var pool = await client.FetchPoolAsync(quote.PoolAddress);

					// Use utility method to create the transaction group
					var redeemTxGroup = TinymanTransaction.PrepareRedeemTransactions(
							appId,
							pool.Asset1,
							pool.Asset2,
							pool.LiquidityAsset,
							quote.Amount,
							account.Address,
							txParams);

					// Sign the transactions sent from the account,
					// the LogicSig transactions will already be signed
					for (var i = 0; i < redeemTxGroup.Transactions.Length; i++) {
						var tx = redeemTxGroup.Transactions[i];

						// Inspect transaction

						// Sign transaction
						if (tx.sender.Equals(account.Address)) {
							redeemTxGroup.SignedTransactions[i] = account.SignTransaction(tx);
						}
					}

					var redeemResult = await client.SubmitAsync(redeemTxGroup);

					Console.WriteLine(
						$"Redeemed {quote.Amount} from {quote.PoolAddress.EncodeAsString()}; transaction ID: {redeemResult.TxId}");
				}

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
