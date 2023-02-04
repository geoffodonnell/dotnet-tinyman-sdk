using Algorand;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V2;
using Account = Algorand.Algod.Model.Account;
using AssetParams = Algorand.Algod.Model.AssetParams;

namespace Tinyman.IntegrationTestConsole {

	internal class Program {

		const string ACCOUNT_ENV_NAME = "ALGORAND_INTEGRATION_TESTNET_ACCOUNT";

		static async Task Main(string[] args) {

			var mnemonic = GetMnemonic();
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

			Console.WriteLine($"Bootstrapping {asset1} <-> {asset2} pool...");
			var bootstrapResult = await client.BootstrapAsync(account, asset1, asset2);
			Console.WriteLine($"Bootstrap complete; tx {bootstrapResult.Txid}.");

			Console.WriteLine($"Fetching pool info...");
			var pool = await client.FetchPoolAsync(asset1, asset2);
			Console.WriteLine($"Fetched pool info.");

			Console.WriteLine($"Opting in to pool liquidity asset...");
			var optInResult = await client.OptInToAssetAsync(account, pool.LiquidityAsset);
			Console.WriteLine($"Opted in to pool liquidity asset.");

			var mintQuote = pool.CalculateMintQuote(new Tuple<AssetAmount, AssetAmount>(
				new AssetAmount(asset1, 1_000_000_000_000),
				new AssetAmount(asset2, 500_000_000_000)));

			Console.WriteLine($"Minting initial liquidity...");
			var mintResult = await client.MintAsync(account, mintQuote);
			Console.WriteLine($"Mint complete; tx {mintResult.Txid}.");

			Console.WriteLine($"Swapping [fixed input] {asset1} <-> {asset2}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote1 = pool.CalculateFixedInputSwapQuote(new AssetAmount(asset1, 10_000), 0.00);
			var swapResult1 = await client.SwapAsync(account, swapQuote1);
			Console.WriteLine($"Swap complete; tx {swapResult1.Txid}.");

			Console.WriteLine($"Swapping [fixed input] {asset2} <-> {asset1}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote2 = pool.CalculateFixedInputSwapQuote(new AssetAmount(asset2, 11_000), 0.00);
			var swapResult2 = await client.SwapAsync(account, swapQuote2);
			Console.WriteLine($"Swap complete; tx {swapResult2.Txid}.");

			Console.WriteLine($"Swapping [fixed output] {asset1} <-> {asset2}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote3 = pool.CalculateFixedOutputSwapQuote(new AssetAmount(asset2, 13_000), 0.00);
			var swapResult3 = await client.SwapAsync(account, swapQuote3);
			Console.WriteLine($"Swap complete; tx {swapResult3.Txid}.");

			Console.WriteLine($"Swapping [fixed output] {asset2} <-> {asset1}...");
			pool = await client.FetchPoolAsync(pool.Address);
			var swapQuote4 = pool.CalculateFixedOutputSwapQuote(new AssetAmount(asset1, 14_000), 0.00);
			var swapResult4 = await client.SwapAsync(account, swapQuote4);
			Console.WriteLine($"Swap complete; tx {swapResult4.Txid}.");

			Console.WriteLine($"Burning liquidity...");
			var liquidityAssetBalance = await client.GetBalanceAsync(account.Address, pool.LiquidityAsset);
			pool = await client.FetchPoolAsync(pool.Address);
			var burnQuote = pool.CalculateBurnQuote(liquidityAssetBalance, 0.00);
			var burnResult = await client.BurnAsync(account, burnQuote);
			Console.WriteLine($"Burned liquidity; tx {burnResult.Txid}");

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

	}

}