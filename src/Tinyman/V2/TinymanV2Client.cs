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

			var validatorAppState = accountInfo
				.AppsLocalState
				.FirstOrDefault()?
				.KeyValue
				.ToDictionary(s => s.Key, s => s.Value);

			var asset1Id = Util.GetStateInt(validatorAppState, "asset_1_id");
			var asset2Id = Util.GetStateInt(validatorAppState, "asset_2_id");

			var asset1 = await FetchAssetAsync(asset1Id.Value);
			var asset2 = await FetchAssetAsync(asset2Id.Value);

			return await CreatePoolFromAccountInfoAsync(accountInfo, asset1, asset2);
		}

		protected virtual async Task<TinymanV2Pool> CreatePoolFromAccountInfoAsync(
			Account accountInfo, Asset asset1, Asset asset2) {

			var expectedAsset1Id = Math.Max(asset1.Id, asset2.Id);
			var expectedAsset2Id = Math.Min(asset1.Id, asset2.Id);
			var poolAddress = accountInfo.Address.EncodeAsString();
			var validatorAppId = accountInfo.AppsLocalState.FirstOrDefault()?.Id;

			if (!validatorAppId.HasValue) {
				return new TinymanV2Pool(asset1, asset2) {
					Exists = false,
					Address = poolAddress,
					Round = accountInfo.Round
				};
			}

			var validatorAppState = accountInfo
				.AppsLocalState
				.FirstOrDefault()?
				.KeyValue
				.ToDictionary(s => s.Key, s => s.Value);

			var asset1Id = Util.GetStateInt(validatorAppState, "asset_1_id");
			var asset2Id = Util.GetStateInt(validatorAppState, "asset_2_id");
			var asset1Reserves = Util.GetStateInt(validatorAppState, "asset_1_reserves");
			var asset2Reserves = Util.GetStateInt(validatorAppState, "asset_2_reserves");
			var issuedLiquidity = Util.GetStateInt(validatorAppState, "issued_pool_tokens");
			var asset1ProtocolFees = Util.GetStateInt(validatorAppState, "asset_1_protocol_fees");
			var asset2ProtocolFees = Util.GetStateInt(validatorAppState, "asset_2_protocol_fees");
			var totalFeeShare = Util.GetStateInt(validatorAppState, "total_fee_share");
			var protocolFeeRatio = Util.GetStateInt(validatorAppState, "protocol_fee_ratio");
			var liquidityAssetId = Util.GetStateInt(validatorAppState, "pool_token_asset_id");

			var liquidityAsset = await FetchAssetAsync((ulong)liquidityAssetId.Value);

			var result = new TinymanV2Pool(asset1, asset2) {
				Exists = true,
				Address = poolAddress,
				LiquidityAsset = liquidityAsset,
				Asset1Reserves = asset1Reserves.GetValueOrDefault(),
				Asset2Reserves = asset2Reserves.GetValueOrDefault(),
				IssuedLiquidity = issuedLiquidity.GetValueOrDefault(),
				Asset1ProtocolFees = asset1ProtocolFees.GetValueOrDefault(),
				Asset2ProtocolFees = asset2ProtocolFees.GetValueOrDefault(),
				TotalFeeShare = totalFeeShare.GetValueOrDefault(),
				ProtocolFeeRatio = protocolFeeRatio.GetValueOrDefault(),
				ValidatorAppId = validatorAppId.GetValueOrDefault(),
				AlgoBalance = accountInfo.Amount,
				Round = accountInfo.Round
			};

			if (asset1Id.GetValueOrDefault() != expectedAsset1Id) {
				throw new Exception(
					$"Expected Pool Asset1 ID of '{expectedAsset1Id}' got {asset1Id.GetValueOrDefault()}");
			}

			if (asset2Id.GetValueOrDefault() != expectedAsset2Id) {
				throw new Exception(
					$"Expected Pool Asset2 ID of '{expectedAsset2Id}' got {asset2Id.GetValueOrDefault()}");
			}

			return result;
		}

	}

}
