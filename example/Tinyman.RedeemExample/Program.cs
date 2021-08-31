using Algorand;
using System;
using System.Configuration;
using Tinyman.V1;
using Tinyman.V1.Action;

namespace Tinyman.RedeemExample {

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

			// Fetch the amounts
			var excessAmounts = client.FetchExcessAmounts(account.Address);

			if (excessAmounts == null || excessAmounts.Count == 0) {
				Console.WriteLine("No excess amounts to redeem");
				return;
			}

			try {

				// Redeem each amount
				foreach (var quote in excessAmounts) {

					var action = Redeem.FromQuote(quote);
					var result = client.Redeem(account, action);

					Console.WriteLine(
						$"Redeemed {quote.Amount} from {quote.PoolAddress.EncodeAsString()}; transaction ID: {result.TxId}");
				}

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
