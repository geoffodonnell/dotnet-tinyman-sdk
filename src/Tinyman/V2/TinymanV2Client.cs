using Algorand;
using Algorand.Algod;
using Algorand.Algod.Model;
using Algorand.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Tinyman.Model;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.V2 {

	/// <summary>
	/// Provides methods for interacting with Tinyman V2 AMM via Algorand REST API.
	/// </summary>
	public class TinymanV2Client : TinymanClient {

		/// <inheritdoc />
		public override string Version => "v2";

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

		/// <inheritdoc />
		public override TransactionGroup PrepareBurnTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			BurnQuote quote) {

			var result = TinymanV2Transaction.PrepareBurnTransactions(
				mValidatorAppId,
				quote.AmountsOutWithSlippage.Item1,
				quote.AmountsOutWithSlippage.Item2,
				quote.LiquidityAssetAmount,
				sender,
				txParams, 
				appCallNote: CreateAppCallNote());

			return result;
		}

		/// <inheritdoc />
		public override TransactionGroup PrepareMintTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			MintQuote quote) {

			var result = TinymanV2Transaction.PrepareMintTransactions(
				mValidatorAppId,
				quote.AmountsIn.Item1,
				quote.AmountsIn.Item2,
				quote.LiquidityAssetAmountWithSlippage,
				sender,
				txParams,
				appCallNote: CreateAppCallNote());

			return result;
		}

		/// <inheritdoc />
		public override TransactionGroup PrepareSwapTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			SwapQuote quote) {

			var amountIn = default(AssetAmount);
			var amountOut = default(AssetAmount);

			if (quote.SwapType == SwapType.FixedInput) {
				amountIn = quote.AmountIn;
				amountOut = quote.AmountOutWithSlippage;
			} else {
				amountIn = quote.AmountInWithSlippage;
				amountOut = quote.AmountOut;
			}

			var result = TinymanV2Transaction.PrepareSwapTransactions(
				mValidatorAppId,
				amountIn,
				amountOut,
				quote.SwapType,
				sender,
				txParams,
				appCallNote: CreateAppCallNote());

			return result;
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
