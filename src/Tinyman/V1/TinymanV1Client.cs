using Algorand;
using Algorand.Algod;
using Algorand.Algod.Model;
using Algorand.Common;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tinyman.Model;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.V1 {

	/// <summary>
	/// Provides methods for interacting with Tinyman V1 AMM via Algorand REST API.
	/// </summary>
	public class TinymanV1Client : TinymanClient {

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		/// <param name="defaultApi">Algod API client</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanV1Client(IDefaultApi defaultApi, ulong validatorAppId)
			: base(defaultApi, validatorAppId) { }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		/// <param name="httpClient">HttpClient configured with appropriate client headers</param>
		/// <param name="url">Algod node base URL</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanV1Client(HttpClient httpClient, string url, ulong validatorAppId)
			: base(httpClient, url, validatorAppId) {
		}

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		/// <param name="url">Algod node base URL</param>
		/// <param name="token">Algod node token</param>
		/// <param name="validatorAppId">Tinyman validator application ID</param>
		public TinymanV1Client(string url, string token, ulong validatorAppId) 
			: base(url, token, validatorAppId) {
		}

		/// <summary>
		/// Retrieve a pool given an asset pair.
		/// </summary>
		/// <param name="asset1">First asset</param>
		/// <param name="asset2">Second asset</param>
		/// <returns>The pool</returns>
		public virtual async Task<TinymanV1Pool> FetchPoolAsync(Asset asset1, Asset asset2) {

			var poolAddress = TinymanV1Contract.GetPoolAddress(mValidatorAppId, asset1.Id, asset2.Id);
			var accountInfo = await mDefaultApi.AccountInformationAsync(poolAddress, null, Format.Json);

			return await CreatePoolFromAccountInfoAsync(accountInfo, asset1, asset2);
		}

		/// <summary>
		/// Retrieve a pool given the pool address.
		/// </summary>
		/// <param name="poolAddress">The pool address</param>
		/// <returns>The pool</returns>
		public virtual async Task<TinymanV1Pool> FetchPoolAsync(Address poolAddress) {

			return await FetchPoolAsync(poolAddress.EncodeAsString());
		}

		/// <summary>
		/// Retrieve a pool given the pool address.
		/// </summary>
		/// <param name="poolAddress">The pool address</param>
		/// <returns>The pool</returns>
		public virtual async Task<TinymanV1Pool> FetchPoolAsync(string poolAddress) {

			var accountInfo = await mDefaultApi
				.AccountInformationAsync(poolAddress, null, Format.Json);

			return await CreatePoolFromAccountInfoAsync(accountInfo);
		}

		/// <summary>
		/// Fetch excess amounts.
		/// </summary>
		/// <param name="address">Address of account</param>
		/// <returns>List of redemption quotes</returns>
		public virtual async Task<List<RedeemQuote>> FetchExcessAmountsAsync(Address address) {

			var result = new List<RedeemQuote>();
			var accountInfo = await mDefaultApi.AccountInformationAsync(address.EncodeAsString(), null, Format.Json);

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
						Asset1 = pool.Asset1,
						Asset2 = pool.Asset2,
						LiquidityAsset = pool.LiquidityAsset
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

			var info = await mDefaultApi.AccountInformationAsync(address.EncodeAsString(), null, null);

			foreach (var entry in info.AppsLocalState) {

				if (entry.Id == mValidatorAppId) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Opt-in to Tinyman.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> OptInAsync(
			Account account,
			bool wait = true) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return await OptInAsync(account, txParams, wait);
		}

		/// <summary>
		/// Opt-in to Tinyman.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> OptInAsync(
			Account account,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareOptInTransactions(
				account.Address, txParams);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		/// <summary>
		/// Opt-out of Tinyman.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> OptOutAsync(
			Account account,
			bool wait = true) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return await OptOutAsync(account, txParams, wait);
		}

		/// <summary>
		/// Opt-out of Tinyman.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> OptOutAsync(
			Account account,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareOptOutTransactions(
				account.Address, txParams);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		/// <summary>
		/// Redeem a specified excess asset amount from a pool.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Redeem quote</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> RedeemAsync(
			Account account,
			RedeemQuote quote,
			bool wait = true) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return await RedeemAsync(account, quote, txParams, wait);
		}

		/// <summary>
		/// Redeem a specified excess asset amount from a pool.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Redeem quote</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> RedeemAsync(
			Account account,
			RedeemQuote quote,
			TransactionParametersResponse txParams,
			bool wait = true) {

			var txs = PrepareRedeemTransactions(
				account.Address, txParams, quote);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		/// <summary>
		/// Prepare a transaction group to opt-in to Tinyman.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareOptInTransactions(
			Address sender) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return PrepareOptInTransactions(sender, txParams);
		}

		/// <summary>
		/// Prepare a transaction group to opt-in to Tinyman.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual TransactionGroup PrepareOptInTransactions(
			Address sender,
			TransactionParametersResponse txParams) {

			var result = TinymanV1Transaction.PrepareAppOptinTransactions(
				mValidatorAppId,
				sender,
				txParams);

			return result;
		}

		/// <summary>
		/// Prepare a transaction group to opt-out of Tinyman.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareOptOutTransactionsAsync(
			Address sender) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return PrepareOptOutTransactions(sender, txParams);
		}

		/// <summary>
		/// Prepare a transaction group to opt-out of Tinyman.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual TransactionGroup PrepareOptOutTransactions(
			Address sender,
			TransactionParametersResponse txParams) {

			var result = TinymanV1Transaction.PrepareAppOptoutTransactions(
				mValidatorAppId,
				sender,
				txParams);

			return result;
		}

		/// <inheritdoc />
		public override TransactionGroup PrepareBurnTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			BurnQuote quote) {

			var result = TinymanV1Transaction.PrepareBurnTransactions(
				mValidatorAppId,
				quote.AmountsOutWithSlippage.Item1,
				quote.AmountsOutWithSlippage.Item2,
				quote.LiquidityAssetAmount,
				sender,
				txParams);

			return result;
		}

		/// <inheritdoc />
		public override TransactionGroup PrepareMintTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			MintQuote quote) {

			var result = TinymanV1Transaction.PrepareMintTransactions(
				mValidatorAppId,
				quote.AmountsIn.Item1,
				quote.AmountsIn.Item2,
				quote.LiquidityAssetAmountWithSlippage,
				sender,
				txParams);

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

			var result = TinymanV1Transaction.PrepareSwapTransactions(
				mValidatorAppId,
				amountIn,
				amountOut,
				quote.LiquidityAsset,
				quote.SwapType,
				sender,
				txParams);

			return result;
		}

		/// <summary>
		/// Prepare a transaction group to redeem a specified excess asset amount from a pool.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="quote">Redeem quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareRedeemTransactionsAsync(
			Address sender,
			RedeemQuote quote) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return PrepareRedeemTransactions(sender, txParams, quote);
		}

		/// <summary>
		/// Prepare a transaction group to redeem a specified excess asset amount from a pool.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="quote">Redeem quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual TransactionGroup PrepareRedeemTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			RedeemQuote quote) {

			var result = TinymanV1Transaction.PrepareRedeemTransactions(
				mValidatorAppId,
				quote.Asset1,
				quote.Asset2,
				quote.LiquidityAsset,
				quote.Amount,
				sender,
				txParams);

			return result;
		}

		protected virtual async Task<TinymanV1Pool> CreatePoolFromAccountInfoAsync(Account accountInfo) {

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

		protected virtual async Task<TinymanV1Pool> CreatePoolFromAccountInfoAsync(
			Account accountInfo, Asset asset1, Asset asset2) {

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

			var poolAddress = TinymanV1Contract.GetPoolAddress(
				validatorAppId.GetValueOrDefault(),
				asset1Id.GetValueOrDefault(),
				asset2Id.GetValueOrDefault());

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

			var result = new TinymanV1Pool(asset1, asset2) {
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

	}

}
