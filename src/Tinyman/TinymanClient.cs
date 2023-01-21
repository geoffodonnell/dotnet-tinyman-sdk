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

	}

}
