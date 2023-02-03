using Algorand.Algod.Model;
using System;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V2;

namespace Tinyman.IntegrationTestConsole {

	internal class Program {

		const string ACCOUNT_ENV_NAME = "ALGORAND_INTEGRATION_TESTNET_ACCOUNT";

		static async Task Main(string[] args) {

			var mnemonic = GetMnemonic();
			var account = new Account(mnemonic);

			// Initialize the client
			var client = new TinymanV2TestnetClient();

			// Assets
			var kollektor = await client.FetchAssetAsync(151333215);
			var algo = await client.FetchAssetAsync(0);
			var lit = await client.FetchAssetAsync(153296445);
			var usdc = await client.FetchAssetAsync(10458941);
			var tinyAu = await client.FetchAssetAsync(21582981);

			// Get the pool - TinyAU <-> ALGO
			var pool1 = await client.FetchPoolAsync(algo, tinyAu);
			var balance1 = await client.GetBalanceAsync(account.Address, pool1.LiquidityAsset);

			// Get the pool - KLTR <-> ALGO
			//var pool2 = await client.FetchPoolAsync(algo, kollektor);
			//var balance2 = await client.GetBalanceAsync(account.Address, pool2.LiquidityAsset);

			var input1 = new AssetAmount(tinyAu, 500); // 0.005
			var input2 = new AssetAmount(algo, 500_000); // 0.5
			var result = pool1.CalculateFlexibleMintQuote(
				new Tuple<AssetAmount, AssetAmount>(input1, input2), 0.005);
			//var quote2 = pool1.CalculateFlexibleMintQuote(, 0.0d);


			//var mintQuote = pool1.CalculateSingleAssetMintQuote(new AssetAmount(algo, 410_010), 0.0d);

			//Console.WriteLine(
			//	$"Minting with {mintQuote.AmountIn} will receive {mintQuote.LiquidityAssetAmount}.");

			//Console.WriteLine("Press enter to execute ...");
			//Console.ReadLine();
			//Console.WriteLine("Executing ...");

			//var bResult = await client.MintAsync(account, mintQuote, true);

			//Console.WriteLine($"Tx ID: {bResult.Txid}");

			//var quote = pool.CalculateMintQuote(new AssetAmount(algo, 1_110_000));
			//var result = await client.MintAsync(account, quote, true);

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

	}

}