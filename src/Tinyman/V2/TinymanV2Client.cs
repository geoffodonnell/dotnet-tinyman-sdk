using Algorand;
using Algorand.Algod;
using Algorand.Algod.Model;
using Algorand.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Tinyman.Model;
using Tinyman.V2.Model;
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

		/// <summary>
		/// Burn the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="account">Account to perform the action</param>
		/// <param name="quote">Burn quote</param>
		/// <param name="wait">Whether or not to wait for the transaction to be confirmed</param>
		/// <returns>Response from node</returns>
		public virtual async Task<PostTransactionsResponse> BurnAsync(
			Account account,
			SingleAssetBurnQuote quote,
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
			SingleAssetBurnQuote quote,
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
			FlexibleMintQuote quote,
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
			FlexibleMintQuote quote,
			TransactionParametersResponse txParams,
			bool wait = true) {

			ValidQuoteOrThrow(quote);

			var txs = PrepareMintTransactions(
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
			SingleAssetMintQuote quote,
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
			SingleAssetMintQuote quote,
			TransactionParametersResponse txParams,
			bool wait = true) {

			ValidQuoteOrThrow(quote);

			var txs = PrepareMintTransactions(
				account.Address, txParams, quote);

			txs.Sign(account);

			return await SubmitAsync(txs, wait);
		}

		/// <inheritdoc />
		public override TransactionGroup PrepareBootstrapTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			Asset asset1,
			Asset asset2) {

			var assets = Util.EnsureOrder(asset1, asset2);
			var minFee = Math.Max(TinymanV2Constant.DefaultMinFee, txParams.MinFee);

			var poolMinimumBalance = 0ul;
			var innerTxCount = 0ul;

			if (assets.Item2.Id == 0) {
				poolMinimumBalance = TinymanV2Constant.MinPoolBalanceAsaAlgoPair;
				innerTxCount = 5;
			} else {
				poolMinimumBalance = TinymanV2Constant.MinPoolBalanceAsaAsaPair;
				innerTxCount = 6;
			}

			var appCallFee = (innerTxCount + 1) * minFee;
			var requiredAlgo = poolMinimumBalance + appCallFee + 100_000;

			var result = TinymanV2Transaction.PrepareBootstrapTransactions(
				mValidatorAppId,
				asset1,
				asset2,
				sender,
				appCallFee,
				txParams,
				requiredAlgo: requiredAlgo,
				appCallNote: CreateAppCallNote());

			return result;
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

		/// <summary>
		/// Prepare a transaction group to burn the liquidity pool asset amount in exchange for a single pool asset.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="quote">Burn quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareBurnTransactionsAsync(
			Address sender,
			SingleAssetBurnQuote quote) {

			var txParams = await mDefaultApi.TransactionParamsAsync();

			return PrepareBurnTransactions(sender, txParams, quote);
		}

		/// <summary>
		/// Prepare a transaction group to burn the liquidity pool asset amount in exchange for a single pool asset.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="quote">Burn quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual TransactionGroup PrepareBurnTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			SingleAssetBurnQuote quote) {

			var otherAsset = quote.SwapQuote.AmountIn.Asset;

			var result = TinymanV2Transaction.PrepareBurnTransactions(
				mValidatorAppId,
				quote.AmountOutWithSlippage,
				new AssetAmount(otherAsset, 0),
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
				isInitialLiquidity: quote.IsInitialLiquidity,
				appCallNote: CreateAppCallNote());

			return result;
		}

		/// <summary>
		/// Prepare a transaction group to mint the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="quote">Mint quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareMintTransactionsAsync(
			Address sender,
			FlexibleMintQuote quote) {

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
		public virtual TransactionGroup PrepareMintTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			FlexibleMintQuote quote) {

			return TinymanV2Transaction.PrepareMintTransactions(
				mValidatorAppId,
				quote.AmountsIn.Item1,
				quote.AmountsIn.Item2,
				quote.LiquidityAssetAmountWithSlippage,
				sender,
				txParams,
				isInitialLiquidity: false,
				appCallNote: CreateAppCallNote());
		}

		/// <summary>
		/// Prepare a transaction group to mint the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="sender">Account address</param>
		/// <param name="quote">Mint quote</param>
		/// <returns>Transaction group to execute action</returns>
		public virtual async Task<TransactionGroup> PrepareMintTransactionsAsync(
			Address sender,
			SingleAssetMintQuote quote) {

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
		public virtual TransactionGroup PrepareMintTransactions(
			Address sender,
			TransactionParametersResponse txParams,
			SingleAssetMintQuote quote) {

			var otherAsset = quote.SwapQuote.AmountOut.Asset;

			return TinymanV2Transaction.PrepareSingleAssetMintTransactions(
				mValidatorAppId,
				quote.AmountIn,
				otherAsset,
				quote.LiquidityAssetAmountWithSlippage,
				sender,
				txParams,
				appCallNote: CreateAppCallNote());
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
					Round = accountInfo.Round,
					ValidatorAppId = mValidatorAppId
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
