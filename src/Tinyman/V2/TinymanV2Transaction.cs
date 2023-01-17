using Algorand;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Common;
using System;
using System.Collections.Generic;
using Tinyman.Model;

namespace Tinyman.V2 {

    public static class TinymanV2Transaction {

		public static TransactionGroup PrepareSwapTransactions(
			ulong validatorAppId,
			AssetAmount amountIn,
			AssetAmount amountOut,
			SwapType swapType,
			Address sender,
			TransactionParametersResponse txParams,
			string appCallNote = null) {

			var transactions = new List<Transaction>();
            var poolAddress = Contract.GetPoolAddress(validatorAppId, amountIn.Asset.Id, amountOut.Asset.Id);
            var minFee = Math.Max(Constant.DefaultMinFee, txParams.MinFee);
            var appCallFee = swapType switch {
                SwapType.FixedInput => minFee * 2, // App call contains 1 inner transaction
				SwapType.FixedOutput => minFee * 3, // App call contains 2 inner transactions
				_ => throw new ArgumentException($"{nameof(swapType)} is not valid.")
			};

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
                    Constant.SwapAppArgument,
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
