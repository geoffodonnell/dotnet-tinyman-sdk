using Algorand;
using Algorand.Common;
using Algorand.V2;
using Algorand.V2.Algod;
using Algorand.V2.Algod.Model;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tinyman.V1.Action;
using Tinyman.V1.Model;
using Account = Algorand.Account;
using AccountModel = Algorand.V2.Algod.Model.Account;
using Asset = Tinyman.V1.Model.Asset;

namespace Tinyman.V1 {

	/// <summary>
	/// Provides methods for interacting with Tinyman AMM via Algorand REST API.
	/// </summary>
	public class TinymanClient {

		protected readonly IDefaultApi mDefaultApi;
		protected readonly HttpClient mHttpClient;
		protected readonly ulong mValidatorAppId;
		protected readonly ConcurrentDictionary<ulong, Asset> mAssetCache;

		public IDefaultApi DefaultApi { get => mDefaultApi; }

		public ulong ValidatorAppId { get => mValidatorAppId; }

		public TinymanClient(
			IDefaultApi defaultApi, ulong validatorAppId) {

			mHttpClient = null;
			mDefaultApi = defaultApi;
			mValidatorAppId = validatorAppId;
			mAssetCache = new ConcurrentDictionary<ulong, Asset>();
		}

		public TinymanClient(
			HttpClient httpClient, string url, ulong validatorAppId) {

			mHttpClient = httpClient;
			mDefaultApi = new DefaultApi(mHttpClient) {
				BaseUrl = url
			};
			mValidatorAppId = validatorAppId;
			mAssetCache = new ConcurrentDictionary<ulong, Asset>();
		}

		public TinymanClient(
			string url, string token, ulong validatorAppId) {

			mHttpClient = HttpClientConfigurator.ConfigureHttpClient(url, token);
			mDefaultApi = new DefaultApi(mHttpClient) {
				BaseUrl = url
			};
			mValidatorAppId = validatorAppId;
			mAssetCache = new ConcurrentDictionary<ulong, Asset>();
		}

		/// <summary>
		/// Retrieve the current network parameters.
		/// </summary>
		/// <returns>Current network parameters</returns>
		public virtual async Task<TransactionParametersResponse> FetchTransactionParamsAsync() {

			return await mDefaultApi.ParamsAsync();
		}

		/// <summary>
		/// Retrieve a pool given an asset pair.
		/// </summary>
		/// <param name="asset1">First asset</param>
		/// <param name="asset2">Second asset</param>
		/// <returns>The pool</returns>
		public virtual async Task<Pool> FetchPoolAsync(Asset asset1, Asset asset2) {

			var poolLogicSig = Contract.GetPoolLogicsigSignature(mValidatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address.EncodeAsString();
			var accountInfo = await mDefaultApi.AccountsAsync(poolAddress, Format.Json);

			return await CreatePoolFromAccountInfoAsync(accountInfo, asset1, asset2);
		}

		/// <summary>
		/// Retrieve a pool given the pool address.
		/// </summary>
		/// <param name="poolAddress">The pool address</param>
		/// <returns>The pool</returns>
		public virtual async Task<Pool> FetchPoolAsync(Address poolAddress) {

			var accountInfo = await mDefaultApi
				.AccountsAsync(poolAddress.EncodeAsString(), Format.Json);

			return await CreatePoolFromAccountInfoAsync(accountInfo);
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
		/// Fetch excess amounts.
		/// </summary>
		/// <param name="address">Address of account</param>
		/// <returns>List of redemption quotes</returns>
		public virtual async Task<List<RedeemQuote>> FetchExcessAmountsAsync(Address address) {

			var result = new List<RedeemQuote>();
			var accountInfo = await mDefaultApi.AccountsAsync(address.EncodeAsString(), Format.Json);

			var validatorApp = accountInfo?
				.AppsLocalState?
				.FirstOrDefault(s => s.Id == mValidatorAppId);

			var validatorAppState = validatorApp?
				.KeyValue?
				.ToDictionary(s => s.Key, s => s.Value);

			if (validatorApp == null || validatorAppState == null) {
				return result;
			}

			foreach (var entry in validatorAppState) {
				var utf8Bytes = Encoding.UTF8.GetBytes(entry.Key);
				var base64Bytes = Base64.Decode(utf8Bytes).ToList();
				var splitOn = Strings.ToUtf8ByteArray("e")[0];

				if (base64Bytes[base64Bytes.Count - 9] == splitOn) {

					var value = (validatorAppState[entry.Key]?.Uint).GetValueOrDefault();
					var poolAddress = new Address(base64Bytes.GetRange(0, base64Bytes.Count - 9).ToArray());
					var assetId = BinaryPrimitives.ReadInt64BigEndian(base64Bytes.GetRange(base64Bytes.Count - 8, 8).ToArray());
					var asset = await FetchAssetAsync((ulong)assetId);
					var pool = await FetchPoolAsync(poolAddress);

					result.Add(new RedeemQuote {
						Amount = new AssetAmount(asset, value),
						PoolAddress = poolAddress,
						Pool = pool
					});
				}
			}

			return result;
		}

		/// <summary>
		/// Check whether or not the address is opted in to Tinyman.
		/// </summary>
		/// <param name="address">Address of account</param>
		/// <returns>Whether or not the address is opted in</returns>
		public virtual async Task<bool> IsOptedInAsync(Address address) {

			var info = await mDefaultApi.AccountsAsync(address.EncodeAsString(), null);

			foreach (var entry in info.AppsLocalState) {

				if (entry.Id == mValidatorAppId) {
					return true;
				}
			}

			return false;
		}

		public virtual async Task<PostTransactionsResponse> OptInAsync(
			Account account,
			bool wait = true) {

			var txParams = await mDefaultApi.ParamsAsync();

			return await OptInAsync(account, txParams, wait);
		}

		public virtual async Task<PostTransactionsResponse> OptInAsync(
			Account account,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareOptInTransactions(
				account.Address, txParams);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		public virtual async Task<PostTransactionsResponse> OptOutAsync(
			Account account,
			bool wait = true) {

			var txParams = await mDefaultApi.ParamsAsync();

			return await OptOutAsync(account, txParams, wait);
		}

		public virtual async Task<PostTransactionsResponse> OptOutAsync(
			Account account,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareOptOutTransactions(
				account.Address, txParams);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		public virtual async Task<PostTransactionsResponse> BurnAsync(
			Account account,
			Burn action,
			bool wait = true) {

			var txParams = await mDefaultApi.ParamsAsync();

			return await BurnAsync(account, action, txParams, wait);
		}

		public virtual async Task<PostTransactionsResponse> BurnAsync(
			Account account,
			Burn action,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareBurnTransactions(
				account.Address, txParams, action);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		[Obsolete("Tinyman V1 pools are vulnerable to liquidity draining attacks; new liquidity should not be provided.")]
		public virtual async Task<PostTransactionsResponse> MintAsync(
			Account account,
			Mint action,
			bool wait = true) {

			var txParams = await mDefaultApi.ParamsAsync();

			return await MintAsync(account, action, txParams, wait);
		}

		[Obsolete("Tinyman V1 pools are vulnerable to liquidity draining attacks; new liquidity should not be provided.")]
		public virtual async Task<PostTransactionsResponse> MintAsync(
			Account account,
			Mint action,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareMintTransactions(
				account.Address, txParams, action);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		public virtual async Task<PostTransactionsResponse> SwapAsync(
			Account account,
			Swap action,
			bool wait = true) {

			var txParams = await mDefaultApi.ParamsAsync();

			return await SwapAsync(account, action, txParams, wait);
		}

		public virtual async Task<PostTransactionsResponse> SwapAsync(
			Account account,
			Swap action,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareSwapTransactions(
				account.Address, txParams, action);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		public virtual async Task<PostTransactionsResponse> RedeemAsync(
			Account account,
			Redeem action,
			bool wait = true) {

			var txParams = await mDefaultApi.ParamsAsync();

			return await RedeemAsync(account, action, txParams, wait);
		}

		public virtual async Task<PostTransactionsResponse> RedeemAsync(
			Account account,
			Redeem action,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareRedeemTransactions(
				account.Address, txParams, action);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		/// <summary>
		/// Convenience method for retreiving an asset balance for an account 
		/// </summary>
		/// <param name="client">Tinyman V1 client</param>
		/// <param name="address">Account address</param>
		/// <param name="asset">Asset</param>
		/// <returns>Asset amount</returns>
		public virtual async Task<AssetAmount> GetBalanceAsync(
			Address address,
			Asset asset) {

			var info = await mDefaultApi.AccountsAsync(address.EncodeAsString(), Format.Json);

			if (asset.Id == 0) {
				return new AssetAmount(asset, Convert.ToUInt64(info.AmountWithoutPendingRewards));
			}

			var amt = info?.Assets?
				.Where(s => s.AssetId == asset.Id)
				.Select(s => s.Amount)
				.FirstOrDefault() ?? 0;

			return new AssetAmount(asset, amt);
		}

		public virtual async Task<TransactionGroup> PrepareOptInTransactions(
			Address sender) {

			var txParams = await mDefaultApi.ParamsAsync();

			return PrepareOptInTransactions(sender, txParams);
		}

		public virtual TransactionGroup PrepareOptInTransactions(
			Address sender,
			TransactionParametersResponse txParams) {

			var result = TinymanTransaction.PrepareAppOptinTransactions(
				mValidatorAppId,
				sender,
				txParams);

			return result;
		}

		public virtual async Task<TransactionGroup> PrepareOptOutTransactionsAsync(
			Address sender) {

			var txParams = await mDefaultApi.ParamsAsync();

			return PrepareOptOutTransactions(sender, txParams);
		}

		public virtual TransactionGroup PrepareOptOutTransactions(
			Address sender, 
			TransactionParametersResponse txParams) {

			var result = TinymanTransaction.PrepareAppOptoutTransactions(
				mValidatorAppId,
				sender,
				txParams);

			return result;
		}

		public virtual async Task<TransactionGroup> PrepareBurnTransactionsAsync(
			Address sender,
			Burn action) {

			var txParams = await mDefaultApi.ParamsAsync();

			return PrepareBurnTransactions(sender, txParams, action);
		}

		public virtual TransactionGroup PrepareBurnTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			Burn action) {

			var result = TinymanTransaction.PrepareBurnTransactions(
				mValidatorAppId,
				action.Amounts.Item1,
				action.Amounts.Item2,
				action.LiquidityAssetAmount,
				sender,
				txParams);

			return result;
		}

		public virtual async Task<TransactionGroup> PrepareMintTransactionsAsync(
			Address sender,
			Mint action) {

			var txParams = await mDefaultApi.ParamsAsync();

			return PrepareMintTransactions(sender, txParams, action);
		}

		public virtual TransactionGroup PrepareMintTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			Mint action) {

			var result = TinymanTransaction.PrepareMintTransactions(
				mValidatorAppId,
				action.Amounts.Item1,
				action.Amounts.Item2,
				action.LiquidityAssetAmount,
				sender,
				txParams);

			return result;
		}

		public virtual async Task<TransactionGroup> PrepareSwapTransactionsAsync(
			Address sender,
			Swap action) {

			var txParams = await mDefaultApi.ParamsAsync();

			return PrepareSwapTransactions(sender, txParams, action);
		}

		public virtual TransactionGroup PrepareSwapTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			Swap action) {

			var result = TinymanTransaction.PrepareSwapTransactions(
				mValidatorAppId,
				action.AmountIn,
				action.AmountOut,
				action.Pool.LiquidityAsset,
				action.SwapType,
				sender,
				txParams);

			return result;
		}

		public virtual async Task<TransactionGroup> PrepareRedeemTransactionsAsync(
			Address sender,
			Redeem action) {

			var txParams = await mDefaultApi.ParamsAsync();

			return PrepareRedeemTransactions(sender, txParams, action);
		}

		public virtual TransactionGroup PrepareRedeemTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			Redeem action) {

			var result = TinymanTransaction.PrepareRedeemTransactions(
				mValidatorAppId,
				action.Pool.Asset1,
				action.Pool.Asset2,
				action.Pool.LiquidityAsset,
				action.Amount,
				sender,
				txParams);

			return result;
		}

		protected virtual async Task<Pool> CreatePoolFromAccountInfoAsync(AccountModel accountInfo) {

			var validatorAppState = accountInfo
				.AppsLocalState
				.FirstOrDefault()?
				.KeyValue
				.ToDictionary(s => s.Key, s => s.Value);

			var asset1Id = Util.GetStateInt(validatorAppState, "a1");
			var asset2Id = Util.GetStateInt(validatorAppState, "a2");

			var asset1 = await FetchAssetAsync(asset1Id.Value);
			var asset2 = await FetchAssetAsync(asset2Id.Value);

			return await CreatePoolFromAccountInfoAsync(accountInfo, asset1, asset2);
		}

		protected virtual async Task<Pool> CreatePoolFromAccountInfoAsync(
			AccountModel accountInfo, Asset asset1, Asset asset2) {

			var validatorAppId = accountInfo.AppsLocalState.FirstOrDefault()?.Id;

			if (!validatorAppId.HasValue) {
				throw new Exception($"'{nameof(validatorAppId)}' not found in pool state.");
			}

			var validatorAppState = accountInfo
				.AppsLocalState
				.FirstOrDefault()?
				.KeyValue
				.ToDictionary(s => s.Key, s => s.Value);

			var asset1Id = Util.GetStateInt(validatorAppState, "a1");
			var asset2Id = Util.GetStateInt(validatorAppState, "a2");

			var poolLogicSig = Contract.GetPoolLogicsigSignature(
				validatorAppId.GetValueOrDefault(),
				asset1Id.GetValueOrDefault(),
				asset2Id.GetValueOrDefault());
			var poolAddress = poolLogicSig.Address.ToString();

			var asset1Reserves = Util.GetStateInt(validatorAppState, "s1");
			var asset2Reserves = Util.GetStateInt(validatorAppState, "s2");
			var issuedLiquidity = Util.GetStateInt(validatorAppState, "ilt");
			var unclaimedProtocolFees = Util.GetStateInt(validatorAppState, "p");

			var liquidityAssetId = accountInfo?
				.CreatedAssets
				.FirstOrDefault()?
				.Index;

			var liquidityAsset = default(Asset);
			var exists = liquidityAssetId != null;

			if (exists) {
				liquidityAsset = await FetchAssetAsync((ulong)liquidityAssetId.Value);
			}

			var outstandingAsset1Amount = Util.GetStateInt(
				validatorAppState, Util.IntToStateKey(asset1Id.GetValueOrDefault()));

			var outstandingAsset2Amount = Util.GetStateInt(
				validatorAppState, Util.IntToStateKey(asset2Id.GetValueOrDefault()));

			var outstandingLiquidityAssetAmount = Util.GetStateInt(
				validatorAppState, Util.IntToStateKey(liquidityAssetId.GetValueOrDefault()));

			var result = new Pool(asset1, asset2) {
				Exists = exists,
				Address = poolAddress,
				LiquidityAsset = liquidityAsset,
				Asset1Reserves = asset1Reserves.GetValueOrDefault(),
				Asset2Reserves = asset2Reserves.GetValueOrDefault(),
				IssuedLiquidity = issuedLiquidity.GetValueOrDefault(),
				UnclaimedProtocolFees = unclaimedProtocolFees.GetValueOrDefault(),
				OutstandingAsset1Amount = outstandingAsset1Amount.GetValueOrDefault(),
				OutstandingAsset2Amount = outstandingAsset2Amount.GetValueOrDefault(),
				OutstandingLiquidityAssetAmount = outstandingLiquidityAssetAmount.GetValueOrDefault(),
				ValidatorAppId = validatorAppId.GetValueOrDefault(),
				AlgoBalance = accountInfo.Amount,
				Round = accountInfo.Round
			};

			if (result.Asset1.Id != asset1Id.GetValueOrDefault()) {
				throw new Exception(
					$"Expected Pool Asset1 ID of '{result.Asset1.Id}' got {Convert.ToInt64(asset1Id.GetValueOrDefault())}");
			}

			if (result.Asset2.Id != asset2Id.GetValueOrDefault()) {
				throw new Exception(
					$"Expected Pool Asset2 ID of '{result.Asset2.Id}' got {Convert.ToInt64(asset2Id.GetValueOrDefault())}");
			}

			return result;
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

			var asset = await mDefaultApi.AssetsAsync(id);

			return new Asset {
				Id = (ulong)asset.Index,
				Name = asset.Params.Name,
				UnitName = asset.Params.UnitName,
				Decimals = asset.Params.Decimals
			};
		}

	}

}
