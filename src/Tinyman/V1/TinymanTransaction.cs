using Algorand;
using Algorand.V2.Model;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tinyman.V1.Model;
using Asset = Tinyman.V1.Model.Asset;
using Transaction = Algorand.Transaction;

namespace Tinyman.V1 {

	/// <summary>
	/// Create transaction groups for common actions
	/// </summary>
	public static class TinymanTransaction {

		/// <summary>
		/// Prepare a transaction group to opt-in to Tinyman
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns></returns>
		public static TransactionGroup PrepareAppOptinTransactions(
			ulong validatorAppId, Address sender, TransactionParametersResponse txParams) {

			var transaction = Algorand.Utils.GetApplicationOptinTransaction(
				sender, Convert.ToUInt64(validatorAppId), txParams);

			return new TransactionGroup(new[] { transaction });
		}

		/// <summary>
		/// Prepare a transaction group to opt-out of Tinyman
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns></returns>
		public static TransactionGroup PrepareAppOptoutTransactions(
			ulong validatorAppId, Address sender, TransactionParametersResponse txParams) {

			var transaction = Algorand.Utils.GetApplicationClearTransaction(
				sender, Convert.ToUInt64(validatorAppId), txParams);

			return new TransactionGroup(new[] { transaction });
		}

		/// <summary>
		/// Prepare a transaction group to opt-in to an asset
		/// </summary>
		/// <param name="assetId">Asset ID</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns></returns>
		public static TransactionGroup PrepareAssetOptinTransactions(
			long assetId, Address sender, TransactionParametersResponse txParams) {

			var transaction = Algorand.Utils.GetAssetOptingInTransaction(
				sender, assetId, txParams);

			return new TransactionGroup(new[] { transaction });
		}

		/// <summary>
		/// Prepare a transaction group to bootstrap a new pool
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="asset1">Asset 1</param>
		/// <param name="asset2">Asset 2</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareBootstrapTransactions(
			ulong validatorAppId,
			Asset asset1,
			Asset asset2,
			Address sender,
			TransactionParametersResponse txParams) {

			var poolLogicSig = Contract.GetPoolLogicsigSignature(validatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address;

			// Id of Asset1 needs to be less than Asset2
			if (asset1.Id < asset2.Id) {
				var placeholder = asset1;

				asset1 = asset2;
				asset2 = placeholder;
			}

			var transactions = new List<Transaction>();

			transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, Convert.ToUInt64(asset2.Id > 0 ? 961000 : 860000), "fee", txParams));

			var appOptinTx = Algorand.Utils.GetApplicationOptinTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), txParams);

			appOptinTx.applicationArgs = new List<byte[]>();
			appOptinTx.applicationArgs.Add(Strings.ToUtf8ByteArray("bootstrap"));
			appOptinTx.applicationArgs.Add(Util.IntToBytes(asset1.Id));
			appOptinTx.applicationArgs.Add(Util.IntToBytes(asset2.Id));

			appOptinTx.foreignAssets.Add(asset1.Id);

			if (asset2.Id != 0) {
				appOptinTx.foreignAssets.Add(asset2.Id);
			}

			transactions.Add(appOptinTx);

			transactions.Add(Algorand.Utils.GetCreateAssetTransaction(new AssetParams() {
				Creator = poolAddress.EncodeAsString(),
				Total = 0xFFFFFFFFFFFFFFFF,
				Decimals = 6,
				UnitName = "TM1POOL",
				Name = $"Tinyman Pool {asset1.UnitName}-{asset2.UnitName}",
				Url = "https://tinyman.org",
				DefaultFrozen = false
			}, txParams));

			transactions.Add(
				Algorand.Utils.GetAssetOptingInTransaction(poolAddress, asset1.Id, txParams));

			if (asset2.Id > 0) {
				transactions.Add(
					Algorand.Utils.GetAssetOptingInTransaction(poolAddress, asset2.Id, txParams));
			}

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		/// <summary>
		/// Prepare a transaction group to burn the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="assetAmount1">Asset 1 amount</param>
		/// <param name="assetAmount2">Asset 2 amount</param>
		/// <param name="assetAmountLiquidity">Liquidity asset amount</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareBurnTransactions(
			ulong validatorAppId,
			AssetAmount assetAmount1,
			AssetAmount assetAmount2,
			AssetAmount assetAmountLiquidity,
			Address sender,
			TransactionParametersResponse txParams) {

			var poolLogicSig = Contract.GetPoolLogicsigSignature(
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
					sender, poolAddress, Constant.BurnFee, "fee", txParams));

			// ApplicationNoOpTxn
			var callTx = Algorand.Utils.GetApplicationCallTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), txParams);

			callTx.onCompletion = OnCompletion.Noop;
			callTx.applicationArgs = new List<byte[]>();
			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray("burn"));
			callTx.accounts.Add(sender);
			callTx.foreignAssets.Add(assetAmount1.Asset.Id);
			callTx.foreignAssets.Add(assetAmountLiquidity.Asset.Id);

			if (assetAmount2.Asset.Id != 0) {
				callTx.foreignAssets.Add(assetAmount2.Asset.Id);
			}

			callTx.foreignAssets = callTx.foreignAssets.OrderBy(s => s).ToList();

			transactions.Add(callTx);

			// AssetTransferTxn
			transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
				poolAddress,
				sender,
				Convert.ToInt64(assetAmount1.Asset.Id),
				Convert.ToUInt64(assetAmount1.Amount),
				txParams));

			// AssetTransferTxn
			if (assetAmount2.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					poolAddress, sender, Convert.ToUInt64(assetAmount2.Amount), "", txParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					poolAddress,
					sender,
					Convert.ToInt64(assetAmount2.Asset.Id),
					Convert.ToUInt64(assetAmount2.Amount),
					txParams));
			}

			// AssetTransferTxn
			transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
				sender,
				poolAddress,
				Convert.ToInt64(assetAmountLiquidity.Asset.Id),
				Convert.ToUInt64(assetAmountLiquidity.Amount),
				txParams));

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		/// <summary>
		/// Prepare a transaction group to mint the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="assetAmount1">Asset 1 amount</param>
		/// <param name="assetAmount2">Asset 2 amount</param>
		/// <param name="assetAmountLiquidity">Liquidity asset amount</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareMintTransactions(
			ulong validatorAppId,
			AssetAmount assetAmount1,
			AssetAmount assetAmount2,
			AssetAmount assetAmountLiquidity,
			Address sender,
			TransactionParametersResponse txParams) {

			var poolLogicSig = Contract.GetPoolLogicsigSignature(
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
					sender, poolAddress, Constant.MintFee, "fee", txParams));

			// ApplicationNoOpTxn
			var callTx = Algorand.Utils.GetApplicationCallTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), txParams);

			callTx.onCompletion = OnCompletion.Noop;
			callTx.applicationArgs = new List<byte[]>();
			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray("mint"));
			callTx.accounts.Add(sender);
			callTx.foreignAssets.Add(assetAmount1.Asset.Id);
			callTx.foreignAssets.Add(assetAmountLiquidity.Asset.Id);

			if (assetAmount2.Asset.Id != 0) {
				callTx.foreignAssets.Add(assetAmount2.Asset.Id);
			}

			callTx.foreignAssets = callTx.foreignAssets.OrderBy(s => s).ToList();

			transactions.Add(callTx);

			// AssetTransferTxn
			transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
				sender,
				poolAddress,
				Convert.ToInt64(assetAmount1.Asset.Id),
				Convert.ToUInt64(assetAmount1.Amount),
				txParams));

			// AssetTransferTxn
			if (assetAmount2.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, Convert.ToUInt64(assetAmount2.Amount), "", txParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					sender,
					poolAddress,
					Convert.ToInt64(assetAmount2.Asset.Id),
					Convert.ToUInt64(assetAmount2.Amount),
					txParams));
			}

			// AssetTransferTxn
			transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
				poolAddress,
				sender,
				Convert.ToInt64(assetAmountLiquidity.Asset.Id),
				Convert.ToUInt64(assetAmountLiquidity.Amount),
				txParams));

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		/// <summary>
		/// Prepare a transaction group to redeem a specified excess asset amount from a pool.
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="asset1">Asset 1</param>
		/// <param name="asset2">Asset 2</param>
		/// <param name="assetLiquidity">Pool liquidity asset</param>
		/// <param name="assetAmount">Asset amount to redeem</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareRedeemTransactions(
			ulong validatorAppId,
			Asset asset1,
			Asset asset2,
			Asset assetLiquidity,
			AssetAmount assetAmount,
			Address sender,
			TransactionParametersResponse txParams) {

			var poolLogicSig = Contract.GetPoolLogicsigSignature(
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
					sender, poolAddress, Constant.RedeemFee, null, txParams));

			// ApplicationNoOpTxn
			var callTx = Algorand.Utils.GetApplicationCallTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), txParams);

			callTx.onCompletion = OnCompletion.Noop;
			callTx.applicationArgs = new List<byte[]>();
			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray("redeem"));
			callTx.accounts.Add(sender);
			callTx.foreignAssets.Add(asset1.Id);
			callTx.foreignAssets.Add(assetLiquidity.Id);

			if (asset2.Id != 0) {
				callTx.foreignAssets.Add(asset2.Id);
			}

			callTx.foreignAssets = callTx.foreignAssets.OrderBy(s => s).ToList();

			transactions.Add(callTx);

			// AssetTransferTxn
			if (assetAmount.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					poolAddress, sender, Convert.ToUInt64(assetAmount.Amount), null, txParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					poolAddress,
					sender,
					assetAmount.Asset.Id,
					assetAmount.Amount,
					txParams));
			}
			
			foreach (var tx in transactions) {
				if (String.IsNullOrWhiteSpace(tx.genesisID)) {
					tx.genesisID = txParams.GenesisId;
				}
			}

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		/// <summary>
		/// Prepare a transaction group to swap assets.
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="amountIn">Amount to send to pool</param>
		/// <param name="amountOut">Amount to receive from pool</param>
		/// <param name="assetLiquidity">Pool liquidity asset</param>
		/// <param name="swapType">Swap type</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareSwapTransactions(
			ulong validatorAppId,
			AssetAmount amountIn,
			AssetAmount amountOut,
			Asset assetLiquidity,
			SwapType swapType,
			Address sender,
			TransactionParametersResponse txParams) {

			var poolLogicSig = Contract.GetPoolLogicsigSignature(
				validatorAppId, amountIn.Asset.Id, amountOut.Asset.Id);
			var poolAddress = poolLogicSig.Address;

			var transactions = new List<Transaction>();

			// PaymentTxn
			transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender,
					poolAddress,
					Constant.SwapFee,
					"fee",
					txParams));

			// ApplicationNoOpTxn
			var callTx = Algorand.Utils.GetApplicationCallTransaction(
				poolAddress, Convert.ToUInt64(validatorAppId), txParams);

			callTx.onCompletion = OnCompletion.Noop;
			callTx.applicationArgs = new List<byte[]>();
			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray("swap"));
			callTx.applicationArgs.Add(Strings.ToUtf8ByteArray(swapType == SwapType.FixedInput ? "fi" : "fo"));
			callTx.accounts.Add(sender);

			if (amountIn.Asset.Id != 0) {
				callTx.foreignAssets.Add(amountIn.Asset.Id);
			}

			if (amountOut.Asset.Id != 0) {
				callTx.foreignAssets.Add(amountOut.Asset.Id);
			}

			callTx.foreignAssets.Add(assetLiquidity.Id);
			callTx.foreignAssets = callTx.foreignAssets.OrderBy(s => s).ToList();

			transactions.Add(callTx);

			// AssetTransferTxn - Send to pool
			if (amountIn.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					sender, poolAddress, amountIn.Amount, null, txParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					sender,
					poolAddress,
					amountIn.Asset.Id,
					amountIn.Amount,
					txParams));
			}

			// AssetTransferTxn - Receive from pool
			if (amountOut.Asset.Id == 0) {
				transactions.Add(Algorand.Utils.GetPaymentTransaction(
					poolAddress, sender, amountOut.Amount, null, txParams));
			} else {
				transactions.Add(Algorand.Utils.GetTransferAssetTransaction(
					poolAddress,
					sender,
					amountOut.Asset.Id,
					amountOut.Amount,
					txParams));
			}

			foreach (var tx in transactions) {
				if (String.IsNullOrWhiteSpace(tx.genesisID)) {
					tx.genesisID = txParams.GenesisId;
				}
			}

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

	}

}
