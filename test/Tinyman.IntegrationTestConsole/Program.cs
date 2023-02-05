using Algorand;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Common;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V2;
using Account = Algorand.Algod.Model.Account;
using AssetParams = Algorand.Algod.Model.AssetParams;

namespace Tinyman.IntegrationTestConsole {

	internal class Program {

		const int DEFAULT_VERSION = 2;
		const string ACCOUNT_ENV_NAME = "ALGORAND_INTEGRATION_TESTNET_ACCOUNT";

		static async Task Main(string[] args) {

			var mnemonic = GetMnemonic();
			var version = GetVersionToRun(args);
			var account = new Account(mnemonic);

			// Initialize the client
			var client = new TinymanV2TestnetClient();
			var txParams = await client.FetchTransactionParamsAsync();
			
			// Create assets for testing
			var asset1Name = Guid.NewGuid().ToString().Replace("-", "");
			var asset2Name = Guid.NewGuid().ToString().Replace("-", "");

			var createAsset1Tx = GetAssetCreateTransaction(asset1Name, account.Address, txParams);
			var createAsset2Tx = GetAssetCreateTransaction(asset2Name, account.Address, txParams);

			// Create and sign the asset creation transactions
			var createAssetsGroup = new TransactionGroup(new[] { createAsset1Tx, createAsset2Tx });
			
			createAssetsGroup.Sign(account);

			// Submit the asset creation transactions
			Console.WriteLine("Creating assets...");
			var createAssetsTxResult = await client.SubmitAsync(createAssetsGroup);
			Console.WriteLine($"Creation complete; tx {createAssetsTxResult.Txid}");

			// Get the asset IDs
			var accountInfo = await client.DefaultApi
				.AccountInformationAsync(account.Address.EncodeAsString(), null, Format.Json);

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
				await V1Integration.PerformSteps(account, asset1, asset2);
			} else if (version == 2) {
				await V2Integration.PerformSteps(account, asset1, asset2);
			} else {
				Console.WriteLine("Provided version is not valid.");
			}

			Console.WriteLine("Press any key to continue ...");
			Console.ReadKey();
		}

		static string GetMnemonic() {

			var found = Environment.GetEnvironmentVariable(ACCOUNT_ENV_NAME, EnvironmentVariableTarget.Process);

			if (String.IsNullOrWhiteSpace(found)) { 
				found = Environment.GetEnvironmentVariable(ACCOUNT_ENV_NAME, EnvironmentVariableTarget.User);
			}

			if (String.IsNullOrWhiteSpace(found)) {
				found = Environment.GetEnvironmentVariable(ACCOUNT_ENV_NAME, EnvironmentVariableTarget.Machine);
			}

			return found?.Replace(",", "") ?? String.Empty;
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

	}

}