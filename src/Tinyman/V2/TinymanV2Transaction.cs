using Algorand;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Common;
using System;
using System.Collections.Generic;
using Tinyman.Model;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.V2 {

	/// <summary>
	/// Create transaction groups for common actions on the Tinyman V2 AMM
	/// </summary>
	public static class TinymanV2Transaction {

		/// <summary>
		/// Prepare a transaction group to bootstrap a new V2 pool
		/// </summary>
		/// <param name="validatorAppId">Tinyman V2 application ID</param>
		/// <param name="asset1">Asset 1</param>
		/// <param name="asset2">Asset 2</param>
		/// <param name="sender">Account address</param>
		/// <param name="appCallFee">Fee for application call transaction</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="requiredAlgo">Fund the required minimum balance</param>
		/// <param name="appCallNote">Note for application call transaction</param>	
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareBootstrapTransactions(
			ulong validatorAppId,
			Asset asset1,
			Asset asset2,
			Address sender,
			ulong appCallFee,
			TransactionParametersResponse txParams,
			ulong? requiredAlgo = null,
			string appCallNote = null) {

			var transactions = new List<Transaction>();
			var poolLogicsigSignature = TinymanV2Contract.GetPoolLogicsigSignature(validatorAppId, asset1.Id, asset2.Id);
			var poolAddress = poolLogicsigSignature.Address;

			var assetIdMax = Math.Max(asset1.Id, asset2.Id);	
			var assetIdMin = Math.Min(asset1.Id, asset2.Id);

			// Pay Txn
			if (requiredAlgo.HasValue) {
				transactions.Add(TxnFactory.Pay(
					sender,
					poolAddress,
					requiredAlgo.Value,
					txParams));
			}

			// App Opt-In Txn
			var appOptInTx = TxnFactory.AppCall(
				poolAddress,
				validatorAppId,
				txParams,
				onCompletion: OnCompletion.Optin,
				applicationArgs: new byte[][] {
					TinymanV2Constant.BootstrapAppArgument
				},
				foreignAssets: new[] {
					assetIdMax,
					assetIdMin
				},
				note: appCallNote.ToApplicationNote(),
				fee: appCallFee);

			appOptInTx.RekeyTo = Address.ForApplication(validatorAppId);
			transactions.Add(appOptInTx);

			var result = new TransactionGroup(transactions);

			result.SignWithLogicSig(poolLogicsigSignature);

			return result;
		}

		/// <summary>
		/// Prepare a transaction group, targeting a V2 pool, to burn the liquidity
		/// pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="validatorAppId">Tinyman V2 application ID</param>
		/// <param name="assetAmount1Minimum">Minimum asset 1 amount</param>
		/// <param name="assetAmount2Minimum">Minimum asset 2 amount</param>
		/// <param name="assetAmountLiquidity">Liquidity asset amount</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="appCallNote">Note for application call transaction</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareBurnTransactions(
			ulong validatorAppId,
			AssetAmount assetAmount1Minimum,
			AssetAmount assetAmount2Minimum,
			AssetAmount assetAmountLiquidity,
			Address sender,
			TransactionParametersResponse txParams,
			string appCallNote = null) {

			var transactions = new List<Transaction>();
			var amounts = Util.EnsureOrder(assetAmount1Minimum, assetAmount2Minimum);
			var poolAddress = TinymanV2Contract.GetPoolAddress(
				validatorAppId, amounts.Item1.Asset.Id, amounts.Item2.Asset.Id);
			var minFee = Math.Max(TinymanV2Constant.DefaultMinFee, txParams.MinFee);
			var appCallFee = minFee * 3; // App call contains 2 inner transactions

			ulong[] foreignAssets;

			// Handling for single asset burns
			if (amounts.Item1.Amount == 0) {
				foreignAssets = new[] { amounts.Item2.Asset.Id };
			} else if (amounts.Item2.Amount == 0) {
				foreignAssets = new[] { amounts.Item1.Asset.Id };
			} else {
				foreignAssets = new[] { amounts.Item1.Asset.Id, amounts.Item2.Asset.Id };
			}

			// Asset Transfer Txn
			transactions.Add(TxnFactory.Pay(
				sender,
				poolAddress,
				assetAmountLiquidity.Amount,
				assetAmountLiquidity.Asset.Id,
				txParams));

			// App Call Txn
			transactions.Add(TxnFactory.AppCall(
				sender,
				validatorAppId,
				txParams,
				applicationArgs: new byte[][] {
					TinymanV2Constant.RemoveLiquidityAppArgument,
					ApplicationArgument.Number(amounts.Item1.Amount),
					ApplicationArgument.Number(amounts.Item2.Amount)
				},
				foreignAssets: foreignAssets,
				accounts: new[] {
					poolAddress
				},
				note: appCallNote.ToApplicationNote(),
				fee: appCallFee));

			return new TransactionGroup(transactions);
		}

		/// <summary>
		/// Prepare a transaction group to mint the liquidity pool asset amount in exchange for pool assets.
		/// </summary>
		/// <param name="validatorAppId">Tinyman V2 application ID</param>
		/// <param name="assetAmount1">Asset 1 amount</param>
		/// <param name="assetAmount2">Asset 2 amount</param>
		/// <param name="assetAmountLiquidityMinimum">Minimum liquidity asset amount</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="isInitialLiquidity">Whether or not the liquidity represents the first liquidity added to the pool</param>
		/// <param name="appCallNote">Note for application call transaction</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareMintTransactions(
			ulong validatorAppId,
			AssetAmount assetAmount1,
			AssetAmount assetAmount2,
			AssetAmount assetAmountLiquidityMinimum,
			Address sender,
			TransactionParametersResponse txParams,
			bool isInitialLiquidity = false,
			string appCallNote = null) {

			var transactions = new List<Transaction>();
			var amounts = Util.EnsureOrder(assetAmount1, assetAmount2);
			var poolAddress = TinymanV2Contract.GetPoolAddress(
				validatorAppId, assetAmount1.Asset.Id, assetAmount2.Asset.Id);
			var minFee = Math.Max(TinymanV2Constant.DefaultMinFee, txParams.MinFee);
			var feeMultiplier = isInitialLiquidity ? 2 : 3; // App call contains 1 or 2 inner transactions
			var appCallFee = minFee * (ulong)feeMultiplier;
			var applicationArgs = isInitialLiquidity ?
				new byte[][] {
					TinymanV2Constant.AddInitialLiquidityAppArgument
				} :
				new byte[][] {
					TinymanV2Constant.AddLiquidityAppArgument,
					TinymanV2Constant.AddLiquidityFlexibleModeAppArgument,
					ApplicationArgument.Number(assetAmountLiquidityMinimum.Amount)
				};

			// Asset Transfer Txn
			transactions.Add(TxnFactory.Pay(
				sender,
				poolAddress,
				amounts.Item1.Amount,
				amounts.Item1.Asset.Id,
				txParams));

			// Asset Transfer Txn
			transactions.Add(TxnFactory.Pay(
				sender,
				poolAddress,
				amounts.Item2.Amount,
				amounts.Item2.Asset.Id,
				txParams));

			// App Call Txn
			transactions.Add(TxnFactory.AppCall(
				sender,
				validatorAppId,
				txParams,
				applicationArgs: applicationArgs,
				foreignAssets: new[] {
					assetAmountLiquidityMinimum.Asset.Id
				},
				accounts: new[] {
					poolAddress
				},
				note: appCallNote.ToApplicationNote(),
				fee: appCallFee));

			return new TransactionGroup(transactions);
		}

		/// <summary>
		/// Prepare a transaction group to mint the liquidity pool asset amount in exchange for a single pool asset.
		/// </summary>
		/// <param name="validatorAppId">Tinyman V2 application ID</param>
		/// <param name="amountIn">Asset amount</param>
		/// <param name="otherAsset">Other pool asset</param>
		/// <param name="assetAmountLiquidityMinimum">Minimum liquidity asset amount</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="appCallNote">Note for application call transaction</param>
		/// <returns>Transaction group to execute action</returns>
		public static TransactionGroup PrepareSingleAssetMintTransactions(
			ulong validatorAppId,
			AssetAmount amountIn,
			Asset otherAsset,
			AssetAmount assetAmountLiquidityMinimum,
			Address sender,
			TransactionParametersResponse txParams,
			string appCallNote = null) {

			var transactions = new List<Transaction>();
			var poolAddress = TinymanV2Contract.GetPoolAddress(
				validatorAppId, amountIn.Asset.Id, otherAsset.Id);
			var minFee = Math.Max(TinymanV2Constant.DefaultMinFee, txParams.MinFee);
			var appCallFee = minFee * 3;

			// Asset Transfer Txn
			transactions.Add(TxnFactory.Pay(
				sender,
				poolAddress,
				amountIn.Amount,
				amountIn.Asset.Id,
				txParams));

			// App Call Txn
			transactions.Add(TxnFactory.AppCall(
				sender,
				validatorAppId,
				txParams,
				applicationArgs: new byte[][] {
					TinymanV2Constant.AddLiquidityAppArgument,
					TinymanV2Constant.AddLiquiditySingleModeAppArgument,
					ApplicationArgument.Number(assetAmountLiquidityMinimum.Amount)
				},
				foreignAssets: new[] {
					assetAmountLiquidityMinimum.Asset.Id
				},
				accounts: new[] {
					poolAddress
				},
				note: appCallNote.ToApplicationNote(),
				fee: appCallFee));

			return new TransactionGroup(transactions);
		}

		/// <summary>
		/// Prepare a transaction group to swap assets using a V2 pool.
		/// </summary>
		/// <param name="validatorAppId">Tinyman V2 application ID</param>
		/// <param name="amountIn">Amount to send to pool</param>
		/// <param name="amountOut">Amount to receive from pool</param>
		/// <param name="swapType">Swap type</param>
		/// <param name="sender">Account address</param>
		/// <param name="txParams">Network parameters</param>
		/// <param name="appCallNote">Note for application call transaction</param>
		/// <returns>Transaction group to execute action</returns>
		/// <exception cref="ArgumentException"></exception>
		public static TransactionGroup PrepareSwapTransactions(
		ulong validatorAppId,
		AssetAmount amountIn,
		AssetAmount amountOut,
		SwapType swapType,
		Address sender,
		TransactionParametersResponse txParams,
		string appCallNote = null) {

			var transactions = new List<Transaction>();
			var poolAddress = TinymanV2Contract.GetPoolAddress(validatorAppId, amountIn.Asset.Id, amountOut.Asset.Id);
			var minFee = Math.Max(TinymanV2Constant.DefaultMinFee, txParams.MinFee);
			ulong appCallFee = 0;
			
			if (swapType == SwapType.FixedInput) {
				appCallFee = minFee * 2; // App call contains 1 inner transaction
			} else if (swapType == SwapType.FixedOutput) {
				appCallFee = minFee * 3; // App call contains 2 inner transactions
			} else {
				throw new ArgumentException($"{nameof(swapType)} is not valid.");
			}

			// Payment Txn
			transactions.Add(TxnFactory.Pay(
				sender,
				poolAddress,
				amountIn.Amount,
				amountIn.Asset.Id,
				txParams));

			// App Call Txn
			transactions.Add(TxnFactory.AppCall(
				sender,
				validatorAppId,
				txParams,
				applicationArgs: new byte[][] {
					TinymanV2Constant.SwapAppArgument,
					swapType.ToApplicationArgument(),
					ApplicationArgument.Number(amountOut.Amount)
				},
				foreignAssets: new[] {
					amountIn.Asset.Id,
					amountOut.Asset.Id
				},
				accounts: new[] {
					poolAddress
				},
				note: appCallNote.ToApplicationNote(),
				fee: appCallFee
			));

			return new TransactionGroup(transactions);
		}

	}

}
