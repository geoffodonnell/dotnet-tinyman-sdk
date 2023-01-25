using Algorand;
using Algorand.Algod;
using Algorand.Algod.Model;
using Algorand.Common;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Tinyman.Model;
using Asset = Tinyman.Model.Asset;

namespace Tinyman {

	/// <summary>
	/// Provides methods for interacting with Tinyman AMM via Algorand REST API.
	/// </summary>
	public abstract class TinymanClient {

		protected const string AppCallNoteTemplate = "tinyman/{0}:j{{\"origin\":\"{1}\"}}";
		protected const string DefaultOrigin = "dotnet-tinyman-sdk";

		protected readonly IDefaultApi mDefaultApi;
		protected readonly HttpClient mHttpClient;
		protected readonly ulong mValidatorAppId;
		protected readonly ConcurrentDictionary<ulong, Asset> mAssetCache;

		/// <summary>
		/// Algod API client
		/// </summary>
		public virtual IDefaultApi DefaultApi { get => mDefaultApi; }

		/// <summary>
		/// Validator application ID
		/// </summary>
		public virtual ulong ValidatorAppId { get => mValidatorAppId; }

		/// <summary>
		/// Dapp version
		/// </summary>
		public abstract string Version { get; }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="defaultApi">Algod API client</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanClient(IDefaultApi defaultApi, ulong validatorAppId) {

			mHttpClient = null;
			mDefaultApi = defaultApi;
			mValidatorAppId = validatorAppId;
			mAssetCache = new ConcurrentDictionary<ulong, Asset>();
		}

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="httpClient">HttpClient configured with appropriate client headers</param>
		/// <param name="url">Algod node base URL</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanClient(HttpClient httpClient, string url, ulong validatorAppId) {

			httpClient.BaseAddress = new Uri(url);

			mHttpClient = httpClient;
			mDefaultApi = new DefaultApi(mHttpClient);
			mValidatorAppId = validatorAppId;
			mAssetCache = new ConcurrentDictionary<ulong, Asset>();
		}

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="url">Algod node base URL</param>
		/// <param name="token">Algod node token</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanClient(string url, string token, ulong validatorAppId) {

			mHttpClient = HttpClientConfigurator.ConfigureHttpClient(url, token);
			mDefaultApi = new DefaultApi(mHttpClient);
			mValidatorAppId = validatorAppId;
			mAssetCache = new ConcurrentDictionary<ulong, Asset>();
		}

		/// <summary>
		/// Retrieve the current network parameters.
		/// </summary>
		/// <returns>Current network parameters</returns>
		public virtual async Task<TransactionParametersResponse> FetchTransactionParamsAsync() {

			return await mDefaultApi.TransactionParamsAsync();
		}

		/// <summary>
		/// Fetch an asset given the asset ID.
		/// </summary>
		/// <param name="id">The asset ID</param>
		/// <returns>The asset</returns>
		public virtual async Task<Asset> FetchAssetAsync(ulong id) {

			if (mAssetCache.TryGetValue(id, out var value)) {
				return value;
			}

			value = await FetchAssetFromApiAsync(id);

			return mAssetCache.GetOrAdd(id, s => value);
		}

		/// <summary>
		/// Burn the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Burn quote</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> BurnAsync(
			Account account,
			BurnQuote quote,
			bool wait = true) {

			ValidQuoteOrThrow(quote);

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return await BurnAsync(account, quote, txParams, wait);
		}

		/// <summary>
		/// Burn the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Burn quote</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> BurnAsync(
			Account account,
			BurnQuote quote,
			TransactionParametersResponse txParams,
			bool wait = true) {

			ValidQuoteOrThrow(quote);

			var txs = PrepareBurnTransactions(
				account.Address, txParams, quote);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		/// <summary>
		/// Mint the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Mint quote</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> MintAsync(
			Account account,
			MintQuote quote,
			bool wait = true) {

			ValidQuoteOrThrow(quote);

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return await MintAsync(account, quote, txParams, wait);
		}

		/// <summary>
		/// Mint the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Mint quote</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> MintAsync(
			Account account,
			MintQuote quote,
			TransactionParametersResponse txParams,
			bool wait = true) {

			ValidQuoteOrThrow(quote);

			var txs = PrepareMintTransactions(
				account.Address, txParams, quote);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}


		/// <summary>
		/// Swap the assets in the provided quote.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Swap quote</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> SwapAsync(
			Account account,
			SwapQuote quote,
			bool wait = true) {

			ValidQuoteOrThrow(quote);

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return await SwapAsync(account, quote, txParams, wait);
		}

		/// <summary>
		/// Swap the assets in the provided quote.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Swap quote</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> SwapAsync(
			Account account,
			SwapQuote quote,
			TransactionParametersResponse txParams,
			bool wait = true) {

			ValidQuoteOrThrow(quote);

			var txs = PrepareSwapTransactions(
				account.Address, txParams, quote);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		/// <summary>
		/// Submit a signed transaction group.
		/// </summary>
		/// <param name="transactionGroup">Signed transaction group</param>
		/// <param name="wait">Wait for confirmation</param>
		/// <returns>Transaction reponse</returns>
		public virtual async Task<PostTransactionsResponse> SubmitAsync(
			TransactionGroup transactionGroup, bool wait = true) {

			return await mDefaultApi
				.SubmitTransactionGroupAsync(transactionGroup, wait);
		}

		/// <summary>
		/// Convenience method for retreiving an asset balance for an account 
		/// </summary>
		/// <param name="address">Account address</param>
		/// <param name="asset">Asset</param>
		/// <returns>Asset amount</returns>
		public virtual async Task<AssetAmount> GetBalanceAsync(
			Address address,
			Asset asset) {

			var info = await mDefaultApi
				.AccountInformationAsync(address.EncodeAsString(), null, Format.Json);

			if (asset.Id == 0) {
				return new AssetAmount(asset, Convert.ToUInt64(info.AmountWithoutPendingRewards));
			}

			var amt = info?.Assets?
				.Where(s => s.AssetId == asset.Id)
				.Select(s => s.Amount)
				.FirstOrDefault() ?? 0;

			return new AssetAmount(asset, amt);
		}

		/// <summary>
		/// Prepare a transaction group to burn the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="quote">Burn quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareBurnTransactionsAsync(
			Address sender,
			BurnQuote quote) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return PrepareBurnTransactions(sender, txParams, quote);
		}

		/// <summary>
		/// Prepare a transaction group to burn the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="quote">Burn quote</param>
		/// <returns>Transaction group to execute action</returns>
		public abstract TransactionGroup PrepareBurnTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			BurnQuote quote);

		/// <summary>
		/// Prepare a transaction group to mint the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="quote">Mint quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareMintTransactionsAsync(
			Address sender,
			MintQuote quote) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return PrepareMintTransactions(sender, txParams, quote);
		}

		/// <summary>
		/// Prepare a transaction group to mint the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="quote">Mint quote</param>
		/// <returns>Transaction group to execute action</returns>
		public abstract TransactionGroup PrepareMintTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			MintQuote quote);

		/// <summary>
		/// Prepare a transaction group to swap assets.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="quote">Swap quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareSwapTransactionsAsync(
			Address sender,
			SwapQuote quote) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return PrepareSwapTransactions(sender, txParams, quote);
		}

		/// <summary>
		/// Prepare a transaction group to swap assets.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="quote">Swap quote</param>
		/// <returns>Transaction group to execute action</returns>
		public abstract TransactionGroup PrepareSwapTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			SwapQuote quote);

		/// <summary>
		/// Get asset information fron the network
		/// </summary>
		/// <param name="id">Asset ID</param>
		/// <returns>Asset</returns>
		protected virtual async Task<Asset> FetchAssetFromApiAsync(ulong id) {

			if (id == 0) {
				return new Asset {
					Id = 0,
					Name = "Algo",
					UnitName = "ALGO",
					Decimals = 6
				};
			}

			var asset = await mDefaultApi.GetAssetByIDAsync(id);

			return new Asset {
				Id = (ulong)asset.Index,
				Name = asset.Params.Name,
				UnitName = asset.Params.UnitName,
				Decimals = (int)asset.Params.Decimals
			};
		}

		/// <summary>
		/// Create a note for the application call transaction.
		/// <br /><br />
		/// More info: <see href="https://github.com/algorandfoundation/ARCs/blob/main/ARCs/arc-0002.md"/>
		/// </summary>
		/// <returns>Application call transaction note</returns>
		protected virtual string CreateAppCallNote() {

			return String.Format(AppCallNoteTemplate, Version, DefaultOrigin);
		}

		/// <summary>
		/// Throws if the quote is not valid for this client.
		/// </summary>
		/// <param name="quote">The operation quote</param>
		/// <exception cref="Exception"></exception>
		protected virtual void ValidQuoteOrThrow(IQuote quote) {

			if (quote?.ValidatorApplicationId != mValidatorAppId) {
				throw new Exception("Quote is not valid for this client.");
			}
		}

	}

}
