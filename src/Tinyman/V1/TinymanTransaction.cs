using Algorand;
using Algorand.V2.Model;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using Tinyman.V1.Model;
using Asset = Tinyman.V1.Model.Asset;
using Transaction = Algorand.Transaction;

namespace Tinyman.V1 {

	public static class TinymanTransaction {

		public static TransactionGroup PrepareAppOptinTransactions(
			ulong validatorAppId, Address sender, TransactionParametersResponse suggestedParams) {

			var transaction = Algorand.Utils.GetApplicationOptinTransaction(
				sender, Convert.ToUInt64(validatorAppId), suggestedParams);

			return new TransactionGroup(new[] { transaction });
		}

		public static TransactionGroup PrepareAppOptoutTransactions(
			ulong validatorAppId, Address sender, TransactionParametersResponse suggestedParams) {

			var transaction = Algorand.Utils.GetApplicationClearTransaction(
				sender, Convert.ToUInt64(validatorAppId), suggestedParams);

			return new TransactionGroup(new[] { transaction });
		}

		public static TransactionGroup PrepareAssetOptinTransactions(
			long assetId, Address sender, TransactionParametersResponse suggestedParams) {

			var transaction = Algorand.Utils.GetAssetOptingInTransaction(
				sender, assetId, suggestedParams);

			return new TransactionGroup(new[] { transaction });
		}

		public static TransactionGroup PrepareBootstrapTransactions(
			ulong validatorAppId,
			Asset asset1,
			Asset asset2,
			Address sender,
			TransactionParametersResponse suggestedParams) {

			var poolLogicSig = Contract.GetPoolLogicSig(validatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address;

			// Swap - Id of Asset1 needs to be less than Asset2
			if (asset1.Id < asset2.Id) {
				var placeholder = asset1;

				asset1 = asset2;
				asset2 = placeholder;
			}

			var transactions = new List<Transaction>();

			transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, Convert.ToUInt64(asset2.Id > 0 ? 961000 : 860000), "fee", suggestedParams));

			var appOptinTx = Algorand.Utils.GetApplicationOptinTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), suggestedParams);

			if (appOptinTx != null) {
				throw new Exception(" Not done yet, I think this should be an app call tx.");
			}

			transactions.Add(Algorand.Utils.GetCreateAssetTransaction(new AssetParams() {
				Creator = poolAddress.EncodeAsString(),
				Total = 0xFFFFFFFFFFFFFFFF,
				Decimals = 6,
				UnitName = "TM1POOL",
				Name = $"Tinyman Pool {asset1.UnitName}{asset2.UnitName}",
				Url = "https://tinyman.org",
				DefaultFrozen = false
			}, suggestedParams));

			transactions.Add(Algorand.Utils.GetAssetOptingInTransaction(poolAddress, asset1.Id, suggestedParams));

			if (asset2.Id > 0) {
				transactions.Add(Algorand.Utils.GetAssetOptingInTransaction(poolAddress, asset2.Id, suggestedParams));
			}

			var result = new TransactionGroup(transactions.ToArray());

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		public static TransactionGroup PrepareBurnTransaction(
			ulong validatorAppId,
			AssetAmount assetAmount1,
			AssetAmount assetAmount2,
			AssetAmount assetAmountLiquidity,
			Address sender,
			TransactionParametersResponse suggestedParams) {

			var poolLogicSig = Contract.GetPoolLogicSig(
				validatorAppId, assetAmount1.Asset.Id, assetAmount2.Asset.Id);
			var poolAddress = poolLogicSig.Address;
			
			if (assetAmount1.Asset.Id < assetAmount2.Asset.Id) {
				var placeholder = assetAmount1;

				assetAmount1 = assetAmount2;
				assetAmount2 = placeholder;
			}

			var transactions = new List<Transaction>();

			// PaymentTxn
			transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, Constants.BurnFee, "fee", suggestedParams));

			// ApplicationNoOpTxn
			var callTx = Algorand.Utils.GetApplicationOptinTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), suggestedParams);

			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray("burn"));
			callTx.accounts.Add(sender);
			callTx.foreignAssets.Add(assetAmount1.Asset.Id);
			callTx.foreignAssets.Add(assetAmountLiquidity.Asset.Id);

			if (assetAmount2.Asset.Id != 0) {
				callTx.foreignAssets.Add(assetAmount2.Asset.Id);
			}

			transactions.Add(callTx);

			// AssetTransferTxn
			transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
				poolAddress,
				sender,
				Convert.ToInt64(assetAmount1.Asset.Id),
				Convert.ToUInt64(assetAmount1.Amount),
				suggestedParams));

			// AssetTransferTxn
			if (assetAmount2.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					poolAddress, sender, Convert.ToUInt64(assetAmount2.Amount), "", suggestedParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					poolAddress,
					sender,
					Convert.ToInt64(assetAmount2.Asset.Id),
					Convert.ToUInt64(assetAmount2.Amount),
					suggestedParams));
			}

			// AssetTransferTxn
			transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
				sender,
				poolAddress,
				Convert.ToInt64(assetAmountLiquidity.Asset.Id),
				Convert.ToUInt64(assetAmountLiquidity.Amount),
				suggestedParams));

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		public static TransactionGroup PrepareMintTransactions(
			ulong validatorAppId,
			AssetAmount assetAmount1,
			AssetAmount assetAmount2,
			AssetAmount assetAmountLiquidity,
			Address sender,
			TransactionParametersResponse suggestedParams) {

			var poolLogicSig = Contract.GetPoolLogicSig(
				validatorAppId, assetAmount1.Asset.Id, assetAmount2.Asset.Id);
			var poolAddress = poolLogicSig.Address;

			if (assetAmount1.Asset.Id < assetAmount2.Asset.Id) {
				var placeholder = assetAmount1;

				assetAmount1 = assetAmount2;
				assetAmount2 = placeholder;
			}

			var transactions = new List<Transaction>();

			// PaymentTxn
			transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, Constants.MintFee, "fee", suggestedParams));

			// ApplicationNoOpTxn
			var callTx = Algorand.Utils.GetApplicationOptinTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), suggestedParams);

			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray("mint"));
			callTx.accounts.Add(sender);
			callTx.foreignAssets.Add(assetAmount1.Asset.Id);
			callTx.foreignAssets.Add(assetAmountLiquidity.Asset.Id);

			if (assetAmount2.Asset.Id != 0) {
				callTx.foreignAssets.Add(assetAmount2.Asset.Id);
			}

			transactions.Add(callTx);

			// AssetTransferTxn
			transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
				sender,
				poolAddress,
				Convert.ToInt64(assetAmount1.Asset.Id),
				Convert.ToUInt64(assetAmount1.Amount),
				suggestedParams));

			// AssetTransferTxn
			if (assetAmount2.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, Convert.ToUInt64(assetAmount2.Amount), "", suggestedParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					sender,
					poolAddress,
					Convert.ToInt64(assetAmount2.Asset.Id),
					Convert.ToUInt64(assetAmount2.Amount),
					suggestedParams));
			}

			// AssetTransferTxn
			transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
				poolAddress,
				sender,
				Convert.ToInt64(assetAmountLiquidity.Asset.Id),
				Convert.ToUInt64(assetAmountLiquidity.Amount),
				suggestedParams));

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		public static TransactionGroup PrepareRedeemTransactions(
			ulong validatorAppId,
			Asset asset1,
			Asset asset2,
			Asset assetLiquidity,
			AssetAmount assetAmount,
			Address sender,
			TransactionParametersResponse suggestedParams) {

			var poolLogicSig = Contract.GetPoolLogicSig(
				validatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address;

			if (asset1.Id < asset2.Id) {
				var placeholder = asset1;

				asset1 = asset2;
				asset2 = placeholder;
			}

			var transactions = new List<Transaction>();
					   			 		  		  		 	   			
			// PaymentTxn
			transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, Constants.RedeemFee, null, suggestedParams));

			// ApplicationNoOpTxn
			var callTx = Algorand.Utils.GetApplicationCallTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), suggestedParams);

			callTx.applicationArgs = new List<byte[]>();
			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray("redeem"));
			callTx.accounts.Add(sender);
			callTx.foreignAssets.Add(asset1.Id);
			callTx.foreignAssets.Add(assetLiquidity.Id);

			if (asset2.Id != 0) {
				callTx.foreignAssets.Add(asset2.Id);
			}

			transactions.Add(callTx);

			// AssetTransferTxn
			if (assetAmount.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					poolAddress, sender, Convert.ToUInt64(assetAmount.Amount), null, suggestedParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					poolAddress,
					sender,
					assetAmount.Asset.Id,
					assetAmount.Amount,
					suggestedParams));
			}
			
			foreach (var tx in transactions) {
				if (String.IsNullOrWhiteSpace(tx.genesisID)) {
					tx.genesisID = suggestedParams.GenesisId;
				}
			}

			var result = new TransactionGroup(transactions);

			var tmp1 = JsonConvert.SerializeObject(transactions, new JsonSerializerSettings() {
				DefaultValueHandling = DefaultValueHandling.Ignore,
				ContractResolver = AlgorandContractResolver.Instance,
				Formatting = Formatting.None
			});

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		public static TransactionGroup PrepareSwapTransactions(
			ulong validatorAppId,
			AssetAmount amountIn,
			AssetAmount amountOut,
			Asset assetLiquidity,
			SwapType swapType,
			Address sender,
			TransactionParametersResponse suggestedParams) {

			var poolLogicSig = Contract.GetPoolLogicSig(
				validatorAppId, amountIn.Asset.Id, amountOut.Asset.Id);
			var poolAddress = poolLogicSig.Address;

			System.Console.WriteLine(
				$"TinymanTransaction.PrepareSwapTransactions() poolAddress={poolAddress}");

			var transactions = new List<Transaction>();

			// PaymentTxn
			transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender,
					poolAddress,
					Constants.SwapFee,
					"fee",
					suggestedParams));

			// ApplicationNoOpTxn
			var callTx = Algorand.Utils.GetApplicationCallTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), suggestedParams);

			callTx.onCompletion = OnCompletion.Noop;
			callTx.applicationArgs = new List<byte[]>();
			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray("swap"));
			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray(swapType == SwapType.FixedInput ? "fi" : "fo"));
			callTx.accounts.Add(sender);
			callTx.foreignAssets.Add(assetLiquidity.Id);

			if (amountIn.Asset.Id != 0) {
				callTx.foreignAssets.Add(amountIn.Asset.Id);
			}

			if (amountOut.Asset.Id != 0) {
				callTx.foreignAssets.Add(amountOut.Asset.Id);
			}

			transactions.Add(callTx);

			// AssetTransferTxn - Send to pool
			if (amountIn.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, amountIn.Amount, "payment", suggestedParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					sender,
					poolAddress,
					amountIn.Asset.Id,
					amountIn.Amount,
					suggestedParams,
					message: "payment"));
			}

			// AssetTransferTxn - Receive from pool
			if (amountOut.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					poolAddress, sender, amountOut.Amount, "payment", suggestedParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					poolAddress,
					sender,
					amountOut.Asset.Id,
					amountOut.Amount,
					suggestedParams,
					message: "payment"));
			}

			foreach (var tx in transactions) {
				if (String.IsNullOrWhiteSpace(tx.genesisID)) {
					tx.genesisID = suggestedParams.GenesisId;
				}
			}

			var result = new TransactionGroup(transactions);

			var tmp1 = JsonConvert.SerializeObject(transactions, new JsonSerializerSettings () {
				DefaultValueHandling = DefaultValueHandling.Ignore,
				ContractResolver = AlgorandContractResolver.Instance,
				Formatting = Formatting.None
			});

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

	}

}
