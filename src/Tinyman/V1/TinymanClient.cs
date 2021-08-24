using Algorand;
using Algorand.V2;
using Algorand.V2.Model;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tinyman.V1.Model;
using Account = Algorand.V2.Model.Account;
using Asset = Tinyman.V1.Model.Asset;

namespace Tinyman.V1 {

	public class TinymanClient {

		protected readonly AlgodApi mAlgodApi;
		protected readonly long mValidatorAppId;
		protected readonly ConcurrentDictionary<long?, Asset> mAssetCache;

		public AlgodApi AlgodApi { get => mAlgodApi; }

		public long ValidatorAppId { get => mValidatorAppId; }

		public TinymanClient(AlgodApi algodApi, long validatorAppId) {

			mAlgodApi = algodApi;
			mValidatorAppId = validatorAppId;
			mAssetCache = new ConcurrentDictionary<long?, Asset>();
		}

		public virtual Pool FetchPool(Asset asset1, Asset asset2) {

			var poolLogicSig = Contract.GetPoolLogicSig(mValidatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address.ToString();
			var accountInfo = mAlgodApi.AccountInformation(poolAddress);

			return FetchPoolInfoFromAccountInfo(accountInfo, asset1, asset2);
		}

		protected virtual Pool FetchPoolInfoFromAccountInfo(
			Account accountInfo, Asset asset1, Asset asset2) {

			var validatorAppId = accountInfo.AppsLocalState[0].Id;
			var validatorAppState = accountInfo.AppsLocalState[0]
				.KeyValue.ToDictionary(s => s.Key, s => s.Value);
			
			var asset1Id = Util.GetStateInt(validatorAppState, "a1");
			var asset2Id = Util.GetStateInt(validatorAppState, "a2");

			var poolLogicSig = Contract.GetPoolLogicSig(
				validatorAppId.GetValueOrDefault(),
				Convert.ToInt64(asset1Id.GetValueOrDefault()),
				Convert.ToInt64(asset2Id.GetValueOrDefault()));
			var poolAddress = poolLogicSig.Address.ToString();

			// TODO: Check

			var asset1Reserves = Util.GetStateInt(validatorAppState, "s1");
			var asset2Reserves = Util.GetStateInt(validatorAppState, "s2");
			var issuedLiquidity = Util.GetStateInt(validatorAppState, "ilt");
			var unclaimedProtocolFees = Util.GetStateInt(validatorAppState, "p");
					   
			var liquidityAsset = accountInfo.CreatedAssets[0];
			var liquidityAssetId = liquidityAsset.Index;
			
			var outstandingAsset1Amount = Util.GetStateInt(
				validatorAppState, Util.IntToStateKey(asset1Id.GetValueOrDefault()));

			var outstandingAsset2Amount = Util.GetStateInt(
				validatorAppState, Util.IntToStateKey(asset2Id.GetValueOrDefault()));

			var outstandingLiquidityAssetAmount = Util.GetStateInt(
				validatorAppState, Util.IntToStateKey(liquidityAssetId.GetValueOrDefault()));

			var result = new Pool(asset1, asset2) {
				Address = poolAddress,
				LiquidityAssetId = liquidityAsset.Index.GetValueOrDefault(),
				LiquidityAssetName = liquidityAsset.Params.Name,
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
				throw new Exception("tbd message");
			}

			if (result.Asset2.Id != Convert.ToInt64(asset2Id.GetValueOrDefault())) {
				throw new Exception("tbd message");
			}

			return result;
		}

		public virtual Asset FetchAsset(long? id) {

			return mAssetCache.GetOrAdd(id, FetchAssetFromApi);
		}

		public virtual PostTransactionsResponse Submit(
			TransactionGroup transactionGroup, bool wait = false) {

			return transactionGroup.Submit(mAlgodApi, wait);
		}

		public virtual object PrepareAppOptinTransactions(Address userAddress) {

			var suggestedParams = mAlgodApi.TransactionParams();
			var transactionGroup = TinymanTransaction.PrepareAppOptinTransactions(
				mValidatorAppId, userAddress, suggestedParams);

			return transactionGroup;
		}

		public virtual Dictionary<string, Pool> FetchExcessAmounts(Address userAddress) {

			var result = new Dictionary<string, Pool>();
			var accountInfo = mAlgodApi.AccountInformation(userAddress.EncodeAsString());

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

					var value = validatorAppState[entry.Key]?.Uint;
					var poolAddress = new Address(base64Bytes.GetRange(base64Bytes.Count - 9, 9).ToArray());

					throw new NotImplementedException("Not implemented yet.");

					/*
				value = validator_app_state[key]['uint']
                pool_address = encode_address(b[:-9])
                pools[pool_address] = pools.get(pool_address, {})
                asset_id = int.from_bytes(b[-8:], 'big')
                asset = self.fetch_asset(asset_id)
                pools[pool_address][asset] = AssetAmount(asset, value)
					 */
				}
			}

			return result;
		}

		public virtual bool IsOptedIn(string address) {

			var info = mAlgodApi.AccountInformation(address);

			foreach (var entry in info.AppsLocalState) {

				if (entry.Id == mValidatorAppId) {
					return true;
				}
			}

			return false;
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
