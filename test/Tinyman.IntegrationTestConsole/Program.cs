using Algorand;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tinyman.V2;
using Account = Algorand.Algod.Model.Account;
using AssetParams = Algorand.Algod.Model.AssetParams;

namespace Tinyman.IntegrationTestConsole {

	internal class Program {

		const int DEFAULT_VERSION = 2;
		const string ACCOUNT_ENV_NAME = "ALGORAND_INTEGRATION_TESTNET_ACCOUNT";
		const string ADDRESS_ENV_NAME = "ALGORAND_INTEGRATION_TESTNET_ADDRESS";
		
		static async Task Main(string[] args) {

			var mnemonic = GetMnemonic();
			var version = GetVersionToRun(args);
			var account = new Account(mnemonic);
			var sender = GetAddress(account);

			// Initialize the client
			var client = new TinymanV2TestnetClient();

			//SEE: https://github.com/FrankSzendzielarz/dotnet-algorand-sdk/issues/11
			ByteArrayConverter.Attach(client.DefaultApi);

			var txParams = await client.FetchTransactionParamsAsync();
			
			// Create assets for testing
			var asset1Name = Guid.NewGuid().ToString().Replace("-", "");
			var asset2Name = Guid.NewGuid().ToString().Replace("-", "");

			var createAsset1Tx = GetAssetCreateTransaction(asset1Name, sender, txParams);
			var createAsset2Tx = GetAssetCreateTransaction(asset2Name, sender, txParams);

			// Create and sign the asset creation transactions
			var createAssetsGroup = new TransactionGroup(new[] { createAsset1Tx, createAsset2Tx });
			
			createAssetsGroup.Sign(account, sender);

			// Submit the asset creation transactions
			Console.WriteLine("Creating assets...");
			var createAssetsTxResult = await client.SubmitAsync(createAssetsGroup);
			Console.WriteLine($"Creation complete; tx {createAssetsTxResult.Txid}");

			// Get the asset IDs
			var accountInfo = await client.DefaultApi
				.AccountInformationAsync(sender.EncodeAsString(), null, Format.Json);

			var asset1Id = accountInfo
				.CreatedAssets
				.FirstOrDefault(s => String.Equals(s.Params.Name, asset1Name, StringComparison.InvariantCulture))?
				.Index;

			var asset2Id = accountInfo
				.CreatedAssets
				.FirstOrDefault(s => String.Equals(s.Params.Name, asset2Name, StringComparison.InvariantCulture))?
				.Index;

			var asset1 = await client.FetchAssetAsync(asset1Id.Value);
			var asset2 = await client.FetchAssetAsync(asset2Id.Value);

			Console.WriteLine($"\t-> {asset1}");
			Console.WriteLine($"\t-> {asset2}");

			if (version == 1) {
				await V1Integration.PerformSteps(account, sender, asset1, asset2);
			} else if (version == 2) {
				await V2Integration.PerformSteps(account, sender, asset1, asset2);
			} else {
				Console.WriteLine("Provided version is not valid.");
			}

			Console.WriteLine("Press any key to continue ...");
			Console.ReadKey();
		}

		static string GetMnemonic() {

			var found = GetEnvironmentVariable(ACCOUNT_ENV_NAME);

			return found?.Replace(",", "");
		}

		static Address GetAddress(Account account) {

			var address =  GetEnvironmentVariable(ADDRESS_ENV_NAME);

			if (!String.IsNullOrWhiteSpace(address)) {
				return new Address(address);
			}

			return account.Address;
		}

		static Transaction GetAssetCreateTransaction(
			string name, 
			Address address,
			TransactionParametersResponse txParams) {

			return TxnFactory.AssetCreate(new AssetParams {
				Clawback = address,
				Creator = address,
				Decimals = 6,
				DefaultFrozen = false,
				Freeze = null,
				Manager = address,
				Name = name,
				UnitName = "GUID",
				Reserve = address,
				Total = 10_000_000_000_000_000
			}, txParams);
		}

		static int GetVersionToRun(string[] args) {

			if (args == null || args.Length == 0) {
				return DEFAULT_VERSION;
			}

			if (String.Equals(args[0], "v0", StringComparison.InvariantCultureIgnoreCase) ||
				String.Equals(args[0], "-v0", StringComparison.InvariantCultureIgnoreCase)) {

				return 0;
			}

			if (String.Equals(args[0], "v1", StringComparison.InvariantCultureIgnoreCase) ||
				String.Equals(args[0], "-v1", StringComparison.InvariantCultureIgnoreCase)) {

				return 1;
			}

			if (String.Equals(args[0], "v2", StringComparison.InvariantCultureIgnoreCase) ||
				String.Equals(args[0], "-v2", StringComparison.InvariantCultureIgnoreCase)) {

				return 2;
			}

			return DEFAULT_VERSION;
		}

		static string GetEnvironmentVariable(string variable) {

			var found = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);

			if (String.IsNullOrWhiteSpace(found)) {
				found = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);
			}

			if (String.IsNullOrWhiteSpace(found)) {
				found = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
			}

			return found ?? String.Empty;
		}
	}

}