using Algorand.Algod.Model;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Tinyman.V1;

namespace Tinyman.RedeemExample {

	class Program {

		static async Task Main(string[] args) {

			var settings = ConfigurationManager.AppSettings;
			var mnemonic = settings.Get("Account.Mnemonic")?.Replace(",", "");

			if (String.IsNullOrWhiteSpace(mnemonic)) {
				throw new Exception("'Account.Mnemonic' key is not set in App.Config.");
			}

			var account = new Account(mnemonic);

			// Initialize the client
			var client = new TinymanV1TestnetClient();

			// Ensure the account is opted in
			var isOptedIn = await client.IsOptedInAsync(account.Address);

			if (!isOptedIn) {
				await client.OptInAsync(account);
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

					var result = await client.RedeemAsync(account, quote);

					Console.WriteLine(
						$"Redeemed {quote.Amount} from {quote.PoolAddress.EncodeAsString()}; transaction ID: {result.Txid}");
				}

			} catch (Exception ex) {
				Console.WriteLine($"An error occured: {ex.Message}");
			}

			Console.WriteLine("Example complete.");
		}

	}

}
