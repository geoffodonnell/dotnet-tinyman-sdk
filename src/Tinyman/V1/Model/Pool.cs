using System;
using System.Collections.Generic;
using System.Text;

namespace Tinyman.V1.Model {

	public class Pool {

		public string Address { get; internal set; }

		public Asset Asset1 { get; internal set; }

		public Asset Asset2 { get; internal set; }

		public long LiquidityAssetId { get; internal set; }

		public string LiquidityAssetName { get; internal set; }

		public long Asset1Reserves { get; internal set;  }

		public long Asset2Reserves { get; internal set; }

		public long IssuedLiquidity { get; internal set; }

		public long UnclaimedProtocolFees { get; internal set; }

		public long OutstandingAsset1Amount { get; internal set; }

		public long OutstandingAsset2Amount { get; internal set; }

		public long OutstandingLiquidityAssetAmount { get; internal set; }

		public long ValidatorAppId { get; internal set; }

		public long AlgoBalance { get; internal set; }

		public long Round { get; internal set; }

		internal Pool(Asset asset1, Asset asset2) {
		
			if (asset1.Id > asset2.Id) {
				Asset1 = asset1;
				Asset2 = asset2;
			} else {
				Asset1 = asset2;
				Asset2 = asset1;
			}
		}

		public SwapQuote FetchFixedInputSwapQuote(
			AssetAmount amountIn, double slippage = 0.05) {

			Asset assetOut;
			long inputSupply;
			long outputSupply;

			if (amountIn.Asset == Asset1) {
				assetOut = Asset2;
				inputSupply = Asset1Reserves;
				outputSupply = Asset2Reserves;
			} else {
				assetOut = Asset1;
				inputSupply = Asset2Reserves;
				outputSupply = Asset1Reserves;
			}

			if (inputSupply == 0 || outputSupply == 0) {
				throw new Exception("Pool has no liquidity!");
			}

			// k = input_supply * output_supply
			// ignoring fees, k must remain constant 
			// (input_supply + asset_in) * (output_supply - amount_out) = k
			var k = (ulong)inputSupply * (ulong)outputSupply;
			var assetInAmountMinusFee = (amountIn.Amount * 997) / (double)1000;
			var swapFees = amountIn.Amount - assetInAmountMinusFee;
			var assetOutAmount = outputSupply - (k / (double)(inputSupply + assetInAmountMinusFee));
			
			var amountOut = new AssetAmount {
				Asset = assetOut,
				Amount = Convert.ToInt64(assetOutAmount)
			};

			var result = new SwapQuote {
				SwapType = SwapType.FixedInput,
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapFees = new AssetAmount {
					Asset = amountIn.Asset,
					Amount = Convert.ToInt64(swapFees)
				},
				Slippage = slippage
			};

			return result;
		}

	}

}
