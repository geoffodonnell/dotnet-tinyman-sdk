using Algorand;
using Algorand.V2;
using Algorand.V2.Model;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tinyman.V1.Model;
using Account = Algorand.V2.Model.Account;
using Asset = Tinyman.V1.Model.Asset;

namespace Tinyman.V1 {

	/// <summary>
	/// Provides methods for interacting with Tinyman AMM via Algorand REST API.
	/// </summary>
	public class TinymanClient {

		protected readonly AlgodApi mAlgodApi;
		protected readonly ulong mValidatorAppId;
		protected readonly ConcurrentDictionary<long?, Asset> mAssetCache;

		internal AlgodApi AlgodApi { get => mAlgodApi; }

		internal ulong ValidatorAppId { get => mValidatorAppId; }

		public TinymanClient(AlgodApi algodApi, ulong validatorAppId) {

			mAlgodApi = algodApi;
			mValidatorAppId = validatorAppId;
			mAssetCache = new ConcurrentDictionary<long?, Asset>();
		}

		/// <summary>
		/// Retrieve a pool given an asset pair.
		/// </summary>
		/// <param name="asset1">First asset</param>
		/// <param name="asset2">Second asset</param>
		/// <returns>The pool</returns>
		public virtual Pool FetchPool(Asset asset1, Asset asset2) {

			var poolLogicSig = Contract.GetPoolLogicsigSignature(mValidatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address.EncodeAsString();
			var accountInfo = mAlgodApi.AccountInformation(poolAddress);

			return FetchPoolInfoFromAccountInfo(accountInfo, asset1, asset2);
		}

		/// <summary>
		/// Retrieve a pool given the pool address.
		/// </summary>
		/// <param name="poolAddress">The pool address</param>
		/// <returns>The pool</returns>
		public virtual Pool FetchPool(Address poolAddress) {

			var accountInfo = mAlgodApi
				.AccountInformation(poolAddress.EncodeAsString());

			return FetchPoolFromAccountInfo(accountInfo);
		}

		/// <summary>
		/// Fetch an asset given the asset ID.
		/// </summary>
		/// <param name="id">The asset ID</param>
		/// <returns>The asset</returns>
		public virtual Asset FetchAsset(long? id) {

			return mAssetCache.GetOrAdd(id, FetchAssetFromApi);
		}

		/// <summary>
		/// Submit a signed transaction group.
		/// </summary>
		/// <param name="transactionGroup">Signed transaction group</param>
		/// <param name="wait">Wait for confirmation</param>
		/// <returns>Transaction reponse</returns>
		public virtual PostTransactionsResponse Submit(
			TransactionGroup transactionGroup, bool wait = true) {

			return transactionGroup.Submit(mAlgodApi, wait);
		}

		/// <summary>
		/// Fetch excess amounts.
		/// </summary>
		/// <param name="address">Address of account</param>
		/// <returns>List of redemption quotes</returns>
		public virtual List<RedeemQuote> FetchExcessAmounts(Address address) {

			var result = new List<RedeemQuote>();
			var appId = Convert.ToInt64(mValidatorAppId);
			var accountInfo = mAlgodApi.AccountInformation(address.EncodeAsString());

			var validatorApp = accountInfo?
				.AppsLocalState?
				.FirstOrDefault(s => s.Id == appId);

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
					var asset = FetchAsset(assetId);

					result.Add(new RedeemQuote {
						Amount = new AssetAmount(asset, value),
						PoolAddress = poolAddress
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
		public virtual bool IsOptedIn(Address address) {

			var info = mAlgodApi.AccountInformation(address.EncodeAsString());
			var appId = Convert.ToInt64(mValidatorAppId);

			foreach (var entry in info.AppsLocalState) {

				if (entry.Id == appId) {
					return true;
				}
			}

			return false;
		}

		protected virtual Pool FetchPoolFromAccountInfo(Account accountInfo) {

			var validatorAppId = accountInfo.AppsLocalState[0].Id;
			var validatorAppState = accountInfo.AppsLocalState[0]
				.KeyValue.ToDictionary(s => s.Key, s => s.Value);

			var asset1Id = Util.GetStateInt(validatorAppState, "a1");
			var asset2Id = Util.GetStateInt(validatorAppState, "a2");

			var asset1 = FetchAsset(Convert.ToInt64(asset1Id));
			var asset2 = FetchAsset(Convert.ToInt64(asset2Id));

			return FetchPoolInfoFromAccountInfo(accountInfo, asset1, asset2);
		}

		protected virtual Pool FetchPoolInfoFromAccountInfo(
			Account accountInfo, Asset asset1, Asset asset2) {

			var validatorAppId = accountInfo.AppsLocalState[0].Id;
			var validatorAppState = accountInfo.AppsLocalState[0]
				.KeyValue.ToDictionary(s => s.Key, s => s.Value);

			var asset1Id = Util.GetStateInt(validatorAppState, "a1");
			var asset2Id = Util.GetStateInt(validatorAppState, "a2");

			var poolLogicSig = Contract.GetPoolLogicsigSignature(
				Convert.ToUInt64(validatorAppId.GetValueOrDefault()),
				Convert.ToInt64(asset1Id.GetValueOrDefault()),
				Convert.ToInt64(asset2Id.GetValueOrDefault()));
			var poolAddress = poolLogicSig.Address.ToString();

			// TODO: Check

			var asset1Reserves = Util.GetStateInt(validatorAppState, "s1");
			var asset2Reserves = Util.GetStateInt(validatorAppState, "s2");
			var issuedLiquidity = Util.GetStateInt(validatorAppState, "ilt");
			var unclaimedProtocolFees = Util.GetStateInt(validatorAppState, "p");

			var liquidityAssetId = accountInfo?.CreatedAssets[0]?.Index.GetValueOrDefault();

			var liquidityAsset = default(Asset);
			var exists = liquidityAssetId != null;

			if (exists) {
				liquidityAsset = FetchAsset(liquidityAssetId);
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
				AlgoBalance = accountInfo.Amount.GetValueOrDefault(),
				Round = accountInfo.Round.GetValueOrDefault()
			};

			if (result.Asset1.Id != Convert.ToInt64(asset1Id.GetValueOrDefault())) {
				throw new Exception(
					$"Expected Pool Asset1 ID of '{result.Asset1.Id}' got {Convert.ToInt64(asset1Id.GetValueOrDefault())}");
			}

			if (result.Asset2.Id != Convert.ToInt64(asset2Id.GetValueOrDefault())) {
				throw new Exception(
					$"Expected Pool Asset2 ID of '{result.Asset2.Id}' got {Convert.ToInt64(asset2Id.GetValueOrDefault())}");
			}

			return result;
		}

		protected virtual Asset FetchAssetFromApi(long? id) {

			if (!id.HasValue || id == 0) {
				return new Asset {
					Id = 0,
					Name = "Algo",
					UnitName = "ALGO",
					Decimals = 6
				};
			}

			var asset = mAlgodApi.GetAssetByID(id);

			return new Asset {
				Id = id.Value,
				Name = asset.Params.Name,
				UnitName = asset.Params.UnitName,
				Decimals = asset.Params.Decimals.GetValueOrDefault()
			};
		}

	}

}
