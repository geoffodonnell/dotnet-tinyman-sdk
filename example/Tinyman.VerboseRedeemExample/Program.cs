using Algorand;
using System;
using System.Configuration;
using Tinyman.V1;
using Tinyman.V1.Model;

namespace Tinyman.VerboseRedeemExample {

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

			// Fetch the amounts
			var excessAmounts = client.FetchExcessAmounts(account.Address);

			if (excessAmounts == null || excessAmounts.Count == 0) {
				Console.WriteLine("No excess amounts to redeem");
				return;
			}

			try {

				// Redeem each amount
				foreach (var quote in excessAmounts) {

					var txParams = algodApi.TransactionParams();
					var pool = client.FetchPool(quote.PoolAddress);

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

					var redeemResult = client.Submit(redeemTxGroup);

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
