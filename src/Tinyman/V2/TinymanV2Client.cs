using Algorand;
using Algorand.Algod;
using Algorand.Algod.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.V2 {

	/// <summary>
	/// Provides methods for interacting with Tinyman V2 AMM via Algorand REST API.
	/// </summary>
	public class TinymanV2Client : TinymanClient {

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		/// <param name="defaultApi">Algod API client</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanV2Client(IDefaultApi defaultApi, ulong validatorAppId)
			: base(defaultApi, validatorAppId) { }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		/// <param name="httpClient">HttpClient configured with appropriate client headers</param>
		/// <param name="url">Algod node base URL</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanV2Client(HttpClient httpClient, string url, ulong validatorAppId)
			: base(httpClient, url, validatorAppId) {
		}

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		/// <param name="url">Algod node base URL</param>
		/// <param name="token">Algod node token</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanV2Client(string url, string token, ulong validatorAppId)
			: base(url, token, validatorAppId) {
		}

		/// <summary>
		/// Retrieve a pool given an asset pair.
		/// </summary>
		/// <param name="asset1">First asset</param>
		/// <param name="asset2">Second asset</param>
		/// <returns>The pool</returns>
		public virtual async Task<TinymanV2Pool> FetchPoolAsync(Asset asset1, Asset asset2) {

			var poolAddress = TinymanV2Contract.GetPoolAddress(mValidatorAppId, asset1.Id, asset2.Id);
			var accountInfo = await mDefaultApi.AccountInformationAsync(poolAddress.EncodeAsString(), null, Format.Json);

			return await CreatePoolFromAccountInfoAsync(accountInfo, asset1, asset2);
		}

		/// <summary>
		/// Retrieve a pool given the pool address.
		/// </summary>
		/// <param name="poolAddress">The pool address</param>
		/// <returns>The pool</returns>
		public virtual async Task<TinymanV2Pool> FetchPoolAsync(Address poolAddress) {

			return await FetchPoolAsync(poolAddress.EncodeAsString());
		}

		/// <summary>
		/// Retrieve a pool given the pool address.
		/// </summary>
		/// <param name="poolAddress">The pool address</param>
		/// <returns>The pool</returns>
		public virtual async Task<TinymanV2Pool> FetchPoolAsync(string poolAddress) {

			var accountInfo = await mDefaultApi
				.AccountInformationAsync(poolAddress, null, Format.Json);

			return await CreatePoolFromAccountInfoAsync(accountInfo);
		}


















		protected virtual async Task<TinymanV2Pool> CreatePoolFromAccountInfoAsync(Account accountInfo) {

			throw new NotImplementedException();

			//var validatorAppState = accountInfo
			//	.AppsLocalState
			//	.FirstOrDefault()?
			//	.KeyValue
			//	.ToDictionary(s => s.Key, s => s.Value);

			//var asset1Id = Util.GetStateInt(validatorAppState, "a1");
			//var asset2Id = Util.GetStateInt(validatorAppState, "a2");

			//var asset1 = await FetchAssetAsync(asset1Id.Value);
			//var asset2 = await FetchAssetAsync(asset2Id.Value);

			//return await CreatePoolFromAccountInfoAsync(accountInfo, asset1, asset2);
		}

		protected virtual async Task<TinymanV2Pool> CreatePoolFromAccountInfoAsync(
			Account accountInfo, Asset asset1, Asset asset2) {

			throw new NotImplementedException();

			//var validatorAppId = accountInfo.AppsLocalState.FirstOrDefault()?.Id;

			//if (!validatorAppId.HasValue) {
			//	throw new Exception($"'{nameof(validatorAppId)}' not found in pool state.");
			//}

			//var validatorAppState = accountInfo
			//	.AppsLocalState
			//	.FirstOrDefault()?
			//	.KeyValue
			//	.ToDictionary(s => s.Key, s => s.Value);

			//var asset1Id = Util.GetStateInt(validatorAppState, "a1");
			//var asset2Id = Util.GetStateInt(validatorAppState, "a2");

			//var poolAddress = TinymanV1Contract.GetPoolAddress(
			//	validatorAppId.GetValueOrDefault(),
			//	asset1Id.GetValueOrDefault(),
			//	asset2Id.GetValueOrDefault());

			//var asset1Reserves = Util.GetStateInt(validatorAppState, "s1");
			//var asset2Reserves = Util.GetStateInt(validatorAppState, "s2");
			//var issuedLiquidity = Util.GetStateInt(validatorAppState, "ilt");
			//var unclaimedProtocolFees = Util.GetStateInt(validatorAppState, "p");

			//var liquidityAssetId = accountInfo?
			//	.CreatedAssets
			//	.FirstOrDefault()?
			//	.Index;

			//var liquidityAsset = default(Asset);
			//var exists = liquidityAssetId != null;

			//if (exists) {
			//	liquidityAsset = await FetchAssetAsync((ulong)liquidityAssetId.Value);
			//}

			//var outstandingAsset1Amount = Util.GetStateInt(
			//	validatorAppState, Util.IntToStateKey(asset1Id.GetValueOrDefault()));

			//var outstandingAsset2Amount = Util.GetStateInt(
			//	validatorAppState, Util.IntToStateKey(asset2Id.GetValueOrDefault()));

			//var outstandingLiquidityAssetAmount = Util.GetStateInt(
			//	validatorAppState, Util.IntToStateKey(liquidityAssetId.GetValueOrDefault()));

			//var result = new TinymanV2Pool(asset1, asset2) {
			//	Exists = exists,
			//	Address = poolAddress,
			//	LiquidityAsset = liquidityAsset,
			//	Asset1Reserves = asset1Reserves.GetValueOrDefault(),
			//	Asset2Reserves = asset2Reserves.GetValueOrDefault(),
			//	IssuedLiquidity = issuedLiquidity.GetValueOrDefault(),
			//	UnclaimedProtocolFees = unclaimedProtocolFees.GetValueOrDefault(),
			//	OutstandingAsset1Amount = outstandingAsset1Amount.GetValueOrDefault(),
			//	OutstandingAsset2Amount = outstandingAsset2Amount.GetValueOrDefault(),
			//	OutstandingLiquidityAssetAmount = outstandingLiquidityAssetAmount.GetValueOrDefault(),
			//	ValidatorAppId = validatorAppId.GetValueOrDefault(),
			//	AlgoBalance = accountInfo.Amount,
			//	Round = accountInfo.Round
			//};

			//if (result.Asset1.Id != asset1Id.GetValueOrDefault()) {
			//	throw new Exception(
			//		$"Expected Pool Asset1 ID of '{result.Asset1.Id}' got {Convert.ToInt64(asset1Id.GetValueOrDefault())}");
			//}

			//if (result.Asset2.Id != asset2Id.GetValueOrDefault()) {
			//	throw new Exception(
			//		$"Expected Pool Asset2 ID of '{result.Asset2.Id}' got {Convert.ToInt64(asset2Id.GetValueOrDefault())}");
			//}

			//return result;
		}

	}

}
