using Algorand;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Common;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tinyman.Model;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.V1 {

    /// <summary>
    /// Create transaction groups for common actions on the Tinyman V1 AMM
    /// </summary>
    public static class TinymanV1Transaction {

		/// <summary>
		/// Prepare a transaction group to opt-in to Tinyman
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareAppOptinTransactions(
			ulong validatorAppId, Address sender, TransactionParametersResponse txParams) {

			var transaction = TxnFactory.AppOptIn(
				sender, validatorAppId, txParams);

			return new TransactionGroup(new[] { transaction });
		}

		/// <summary>
		/// Prepare a transaction group to opt-out of Tinyman
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareAppOptoutTransactions(
			ulong validatorAppId, Address sender, TransactionParametersResponse txParams) {

			var transaction = TxnFactory.AppCall(
				sender, validatorAppId, txParams, onCompletion: OnCompletion.Clear);

			return new TransactionGroup(new[] { transaction });
		}

		/// <summary>
		/// Prepare a transaction group to opt-in to an asset
		/// </summary>
		/// <param name="assetId">Asset ID</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareAssetOptinTransactions(
			ulong assetId, Address sender, TransactionParametersResponse txParams) {

			var transaction = TxnFactory.AssetOptIn(
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

			var poolLogicSig = TinymanV1Contract.GetPoolLogicsigSignature(validatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address;

			// Id of Asset1 needs to be less than Asset2
			if (asset1.Id < asset2.Id) {
				var placeholder = asset1;

				asset1 = asset2;
				asset2 = placeholder;
			}

			var transactions = new List<Transaction>();

			transactions.Add(TxnFactory.Pay(
					sender, 
					poolAddress,
					asset2.Id > 0 ? 961000ul : 860000ul,
					txParams,
					note: Strings.ToUtf8ByteArray("fee")));

			// AppCall
			var applicationArgs = new byte[][] {
				Strings.ToUtf8ByteArray("bootstrap"),
				Util.IntToBytes(asset1.Id),
				Util.IntToBytes(asset2.Id)
			};

			var foreignAssets = new List<ulong>() { asset1.Id };

			if (asset2.Id != 0) {
				foreignAssets.Add(asset2.Id);
			}

			var appOptinTx = TxnFactory.AppCall(
				poolAddress,
				validatorAppId,
				txParams, 
				onCompletion: OnCompletion.Optin,
				applicationArgs: applicationArgs,
				foreignAssets: foreignAssets.ToArray());

			transactions.Add(appOptinTx);

			var name = String.Empty;
			var unitName = String.Empty;

			if (validatorAppId == TinymanV1Constant.TestnetValidatorAppIdV1_0 ||
				validatorAppId == TinymanV1Constant.MainnetValidatorAppIdV1_0) {
				unitName = "TM1POOL";
				name = $"Tinyman Pool {asset1.UnitName}-{asset2.UnitName}";
			} else {
				unitName = "TMPOOL11";
				name = $"TinymanPool1.1 {asset1.UnitName}-{asset2.UnitName}";
			}

			var metadataHash = Encoding.UTF8.GetBytes(
				Algorand.Utils.Utils.GetRandomAssetMetaHash()).Take(32).ToArray();

			transactions.Add(TxnFactory.AssetCreate(new AssetParams() {
				Creator = poolAddress,
				Total = 0xFFFFFFFFFFFFFFFF,
				Decimals = 6,
				UnitName = unitName,
				Name = name,
				Url = "https://tinyman.org",
				DefaultFrozen = false,
				MetadataHash = metadataHash
			}, txParams));

			transactions.Add(TxnFactory.AssetOptIn(poolAddress, asset1.Id, txParams));

			if (asset2.Id > 0) {
				transactions.Add(TxnFactory.AssetOptIn(poolAddress, asset2.Id, txParams));
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

			var poolLogicSig = TinymanV1Contract.GetPoolLogicsigSignature(
				validatorAppId, assetAmount1.Asset.Id, assetAmount2.Asset.Id);
			var poolAddress = poolLogicSig.Address;
			
			if (assetAmount1.Asset.Id < assetAmount2.Asset.Id) {
				var placeholder = assetAmount1;

				assetAmount1 = assetAmount2;
				assetAmount2 = placeholder;
			}

			var transactions = new List<Transaction>();

			// PaymentTxn
			transactions.Add(TxnFactory.Pay(
					sender,
					poolAddress,
					TinymanV1Constant.BurnFee,
					txParams,
					note: Strings.ToUtf8ByteArray("fee")));

			// ApplicationNoOpTxn
			var applicationArgs = new byte[][] {
				Strings.ToUtf8ByteArray("burn")
			};
			var accounts = new Address[] {
				sender
			};
			var foreignAssets = new List<ulong>() {
				assetAmount1.Asset.Id,
				assetAmountLiquidity.Asset.Id
			};

			if (assetAmount2.Asset.Id != 0) {
				foreignAssets.Add(assetAmount2.Asset.Id);
			}

			var callTx = TxnFactory.AppCall(
				poolAddress,
				Convert.ToUInt64(validatorAppId),
				txParams,
				accounts: accounts,
				applicationArgs: applicationArgs,
				foreignAssets: foreignAssets.OrderBy(s => s).ToArray());

			transactions.Add(callTx);

			// AssetTransferTxn
			transactions.Add(TxnFactory.Pay(
				poolAddress,
				sender,
				assetAmount1.Amount,
				assetAmount1.Asset.Id,
				txParams));

			// AssetTransferTxn
			if (assetAmount2.Asset.Id == 0) {
				transactions.Add(TxnFactory.Pay(
					poolAddress, sender, assetAmount2.Amount, txParams));
			} else {
				transactions.Add(TxnFactory.Pay(
					poolAddress,
					sender,
					assetAmount2.Amount,
					assetAmount2.Asset.Id,
					txParams));
			}

			// AssetTransferTxn
			transactions.Add(TxnFactory.Pay(
				sender,
				poolAddress,
				assetAmountLiquidity.Amount,
				assetAmountLiquidity.Asset.Id,
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

			var poolLogicSig = TinymanV1Contract.GetPoolLogicsigSignature(
				validatorAppId, assetAmount1.Asset.Id, assetAmount2.Asset.Id);
			var poolAddress = poolLogicSig.Address;

			if (assetAmount1.Asset.Id < assetAmount2.Asset.Id) {
				var placeholder = assetAmount1;

				assetAmount1 = assetAmount2;
				assetAmount2 = placeholder;
			}

			var transactions = new List<Transaction>();

			// PaymentTxn
			transactions.Add(TxnFactory.Pay(
					sender,
					poolAddress,
					TinymanV1Constant.MintFee,
					txParams,
					note: Strings.ToUtf8ByteArray("fee")));

			// ApplicationNoOpTxn
			var applicationArgs = new byte[][] {
				Strings.ToUtf8ByteArray("mint")
			};
			var accounts = new Address[] {
				sender
			};
			var foreignAssets = new List<ulong>() {
				assetAmount1.Asset.Id,
				assetAmountLiquidity.Asset.Id
			};

			if (assetAmount2.Asset.Id != 0) {
				foreignAssets.Add(assetAmount2.Asset.Id);
			}

			var callTx = TxnFactory.AppCall(
				poolAddress,
				validatorAppId,
				txParams,
				accounts: accounts,
				applicationArgs: applicationArgs,
				foreignAssets: foreignAssets.OrderBy(s => s).ToArray());

			transactions.Add(callTx);

			// AssetTransferTxn
			transactions.Add(TxnFactory.Pay(
				sender,
				poolAddress,
				assetAmount1.Amount,
				assetAmount1.Asset.Id,
				txParams));

			// AssetTransferTxn
			if (assetAmount2.Asset.Id == 0) {
				transactions.Add(TxnFactory.Pay(
					sender, poolAddress, assetAmount2.Amount, txParams));
			} else {
				transactions.Add(TxnFactory.Pay(
					sender,
					poolAddress,
					assetAmount2.Amount,
					assetAmount2.Asset.Id,
					txParams));
			}

			// AssetTransferTxn
			transactions.Add(TxnFactory.Pay(
				poolAddress,
				sender,
				assetAmountLiquidity.Amount,
				assetAmountLiquidity.Asset.Id,
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

			var poolLogicSig = TinymanV1Contract.GetPoolLogicsigSignature(
				validatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address;

			if (asset1.Id < asset2.Id) {
				var placeholder = asset1;

				asset1 = asset2;
				asset2 = placeholder;
			}

			var transactions = new List<Transaction>();
					   			 		  		  		 	   			
			// PaymentTxn
			transactions.Add(TxnFactory.Pay(
					sender, poolAddress, TinymanV1Constant.RedeemFee, txParams));

			// ApplicationNoOpTxn
			var applicationArgs = new byte[][] {
				Strings.ToUtf8ByteArray("redeem")
			};
			var accounts = new Address[] {
				sender
			};
			var foreignAssets = new List<ulong>() {
				asset1.Id,
				assetLiquidity.Id
			};

			if (asset2.Id != 0) {
				foreignAssets.Add(asset2.Id);
			}

			var callTx = TxnFactory.AppCall(
				poolAddress,
				validatorAppId,
				txParams,
				accounts: accounts,
				applicationArgs: applicationArgs,
				foreignAssets: foreignAssets.OrderBy(s => s).ToArray());

			transactions.Add(callTx);

			// AssetTransferTxn
			if (assetAmount.Asset.Id == 0) {
				transactions.Add(TxnFactory.Pay(
					poolAddress, sender, assetAmount.Amount, txParams));
			} else {
				transactions.Add(TxnFactory.Pay(
					poolAddress,
					sender,
					assetAmount.Amount,
					assetAmount.Asset.Id,
					txParams));
			}
			
			foreach (var tx in transactions) {
				if (String.IsNullOrWhiteSpace(tx.GenesisID)) {
					tx.GenesisID = txParams.GenesisId;
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

			var poolLogicSig = TinymanV1Contract.GetPoolLogicsigSignature(
				validatorAppId, amountIn.Asset.Id, amountOut.Asset.Id);
			var poolAddress = poolLogicSig.Address;

			var transactions = new List<Transaction>();

			// PaymentTxn
			transactions.Add(TxnFactory.Pay(
					sender,
					poolAddress,
					TinymanV1Constant.SwapFee,
					txParams,
					note: Strings.ToUtf8ByteArray("fee")));

			// ApplicationNoOpTxn
			var applicationArgs = new byte[][] {
				Strings.ToUtf8ByteArray("swap"),
				Strings.ToUtf8ByteArray(swapType == SwapType.FixedInput ? "fi" : "fo")
			};

			var accounts = new Address[] {
				sender
			};
			var foreignAssets = new List<ulong>();

			if (amountIn.Asset.Id != 0) {
				foreignAssets.Add(amountIn.Asset.Id);
			}

			if (amountOut.Asset.Id != 0) {
				foreignAssets.Add(amountOut.Asset.Id);
			}

			foreignAssets.Add(assetLiquidity.Id);

			var callTx = TxnFactory.AppCall(
				poolAddress,
				validatorAppId,
				txParams,
				accounts: accounts,
				applicationArgs: applicationArgs,
				foreignAssets: foreignAssets.OrderBy(s => s).ToArray());

			transactions.Add(callTx);

			// AssetTransferTxn - Send to pool
			if (amountIn.Asset.Id == 0) {
				transactions.Add(TxnFactory.Pay(
					sender, poolAddress, amountIn.Amount, txParams));
			} else {
				transactions.Add(TxnFactory.Pay(
					sender,
					poolAddress,
					amountIn.Amount,
					amountIn.Asset.Id,
					txParams));
			}

			// AssetTransferTxn - Receive from pool
			if (amountOut.Asset.Id == 0) {
				transactions.Add(TxnFactory.Pay(
					poolAddress, sender, amountOut.Amount, txParams));
			} else {
				transactions.Add(TxnFactory.Pay(
					poolAddress,
					sender,
					amountOut.Amount,
					amountOut.Asset.Id,
					txParams));
			}

			foreach (var tx in transactions) {
				if (String.IsNullOrWhiteSpace(tx.GenesisID)) {
					tx.GenesisID = txParams.GenesisId;
				}
			}

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

		/// <summary>
		/// Redeem pool fees
		/// </summary>
		/// <param name="validatorAppId">Tinyman application ID</param>
		/// <param name="asset1">Asset 1</param>
		/// <param name="asset2">Asset 2</param>
		/// <param name="assetLiquidity">Pool liquidity asset</param>
		/// <param name="assetAmount">Asset amount to redeem</param>
		/// <param name="creator">Creator</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareRedeemFeesTransactions(
			ulong validatorAppId,
			Asset asset1,
			Asset asset2,
			Asset assetLiquidity,
			AssetAmount assetAmount,
			Address creator,
			Address sender,
			TransactionParametersResponse txParams) {

			if (assetLiquidity.Id != assetAmount.Asset.Id) {
				throw new Exception("Amount to redeem must be liquidity pool asset.");
			}

			var poolLogicSig = TinymanV1Contract.GetPoolLogicsigSignature(
				validatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicSig.Address;

			var transactions = new List<Transaction>();	

			// PaymentTxn
			transactions.Add(TxnFactory.Pay(
					sender,
					poolAddress,
					TinymanV1Constant.RedeemFeesFee,
					txParams,
					note: Strings.ToUtf8ByteArray("fee")));

			// ApplicationNoOpTxn			
			var callTx = TxnFactory.AppCall(
				poolAddress, validatorAppId, txParams);

			if (callTx is ApplicationNoopTransaction noopTx) {
				noopTx.ApplicationArgs = new List<byte[]> {
					Strings.ToUtf8ByteArray("fees")
				};

				noopTx.ForeignAssets = new List<ulong> {
					asset1.Id
				};

				if (asset2.Id != 0) {
					noopTx.ForeignAssets.Add(asset2.Id);
				}

				noopTx.ForeignAssets.Add(assetLiquidity.Id);
			}

			transactions.Add(callTx);

			// AssetTransferTxn - Receive from pool
			transactions.Add(TxnFactory.Pay(
				poolAddress,
				creator,
				assetAmount.Amount,
				assetAmount.Asset.Id,
				txParams));

			foreach (var tx in transactions) {
				if (String.IsNullOrWhiteSpace(tx.GenesisID)) {
					tx.GenesisID = txParams.GenesisId;
				}
			}

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicSig);

			return result;
		}

	}

}
