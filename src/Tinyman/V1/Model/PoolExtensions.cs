using Algorand;
using System;
using System.Numerics;
using Tinyman.V1.Action;

namespace Tinyman.V1.Model {

	public static class PoolExtensions {

		public static SwapQuote CalculateFixedInputSwapQuote(
			this Pool pool, 
			AssetAmount amountIn,
			double slippage = 0.05) {

			Asset assetOut;
			ulong inputSupply;
			ulong outputSupply;

			if (amountIn.Asset == pool.Asset1) {
				assetOut = pool.Asset2;
				inputSupply = pool.Asset1Reserves;
				outputSupply = pool.Asset2Reserves;
			} else {
				assetOut = pool.Asset1;
				inputSupply = pool.Asset2Reserves;
				outputSupply = pool.Asset1Reserves;
			}

			if (inputSupply == 0 || outputSupply == 0) {
				throw new Exception("Pool has no liquidity!");
			}

			// k = input_supply * output_supply
			// ignoring fees, k must remain constant 
			// (input_supply + asset_in) * (output_supply - amount_out) = k
			var k = BigInteger.Multiply(inputSupply, outputSupply);
			var assetInAmountMinusFee = Convert.ToUInt64(amountIn.Amount * 997d / 1000d);
			var swapFees = amountIn.Amount - assetInAmountMinusFee;
			var assetOutAmount = (ulong)(outputSupply - (k / (inputSupply + assetInAmountMinusFee)));

			var amountOut = new AssetAmount {
				Asset = assetOut,
				Amount = assetOutAmount
			};

			var result = new SwapQuote {
				SwapType = SwapType.FixedInput,
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapFees = new AssetAmount {
					Asset = amountIn.Asset,
					Amount = swapFees
				},
				Slippage = slippage
			};

			return result;
		}

		public static SwapQuote CalculateFixedOutputSwapQuote(
			this Pool pool, 
			AssetAmount amountOut,
			double slippage = 0.05) {

			Asset assetIn;
			ulong inputSupply;
			ulong outputSupply;

			if (amountOut.Asset == pool.Asset1) {
				assetIn = pool.Asset2;
				inputSupply = pool.Asset2Reserves;
				outputSupply = pool.Asset1Reserves;
			} else {
				assetIn = pool.Asset1;
				inputSupply = pool.Asset1Reserves;
				outputSupply = pool.Asset2Reserves;
			}

			if (inputSupply == 0 || outputSupply == 0) {
				throw new Exception("Pool has no liquidity!");
			}

			// k = input_supply * output_supply
			// ignoring fees, k must remain constant 
			// (input_supply + asset_in) * (output_supply - amount_out) = k
			var k = BigInteger.Multiply(inputSupply, outputSupply);
			var calculatedAmountInWithoutFee = (ulong)((k / (outputSupply - amountOut.Amount)) - inputSupply);
			var assetInAmount = calculatedAmountInWithoutFee * 1000d / 997d;
			var swapFees = assetInAmount - calculatedAmountInWithoutFee;

			var amountIn = new AssetAmount {
				Asset = assetIn,
				Amount = Convert.ToUInt64(assetInAmount)
			};

			var result = new SwapQuote {
				SwapType = SwapType.FixedOutput,
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapFees = new AssetAmount {
					Asset = amountIn.Asset,
					Amount = Convert.ToUInt64(swapFees)
				},
				Slippage = slippage
			};

			return result;
		}

	}

}