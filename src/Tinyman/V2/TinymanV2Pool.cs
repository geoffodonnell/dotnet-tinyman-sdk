using System;
using System.Numerics;
using Tinyman.Model;

namespace Tinyman.V2 {

	public class TinymanV2Pool : Pool {

		public virtual ulong Asset1ProtocolFees { get; set; }

		public virtual ulong Asset2ProtocolFees { get; set; }

		public virtual ulong TotalFeeShare { get; set; }

		public virtual ulong ProtocolFeeRatio { get; set; }

		public TinymanV2Pool() { }

		internal TinymanV2Pool(Asset asset1, Asset asset2) {

			if (asset1.Id > asset2.Id) {
				Asset1 = asset1;
				Asset2 = asset2;
			} else {
				Asset1 = asset2;
				Asset2 = asset1;
			}

			Exists = false;
		}

		public virtual AssetAmount Convert(AssetAmount amount) {

			if (amount.Asset == Asset1) {
				return new AssetAmount(Asset2, amount.Amount * Asset1Price);
			}

			if (amount.Asset == Asset2) {
				return new AssetAmount(Asset1, amount.Amount * Asset2Price);
			}

			return null;
		}

		public virtual SwapQuote CalculateFixedInputSwapQuote(
			AssetAmount amountIn,
			double slippage = 0.005) {

			Asset assetOut;
			ulong inputSupply;
			ulong outputSupply;

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

			var totalFeeAmount = CalculateFixedInputFeeAmount(amountIn.Amount);
			var swapAmount = amountIn.Amount - totalFeeAmount;
			
			if (!TryCalculateOutputAmountOfFixedInputSwap(inputSupply, outputSupply, swapAmount, out var assetOutAmount)) {
				throw new Exception("Insufficient reserves");
			}

			var priceImpact = CalculatePriceImpact(inputSupply, outputSupply, amountIn.Amount, assetOutAmount);
			var amountOut = new AssetAmount(assetOut, assetOutAmount);
			var swapFees = new AssetAmount(amountIn.Asset, totalFeeAmount);

			var result = new SwapQuote() {
				SwapType = SwapType.FixedInput,
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapFees = swapFees,
				Slippage = slippage,
				PriceImpact = priceImpact,
				LiquidityAsset = LiquidityAsset
			};

			return result;
		}

		public virtual SwapQuote CalculateFixedOutputSwapQuote(
			AssetAmount amountOut,
			double slippage = 0.005) {

			Asset assetIn;
			ulong inputSupply;
			ulong outputSupply;

			if (amountOut.Asset == Asset1) {
				assetIn = Asset2;
				inputSupply = Asset2Reserves;
				outputSupply = Asset1Reserves;
			} else {
				assetIn = Asset1;
				inputSupply = Asset1Reserves;
				outputSupply = Asset2Reserves;
			}

			if (inputSupply == 0 || outputSupply == 0) {
				throw new Exception("Pool has no liquidity!");
			}

			if (outputSupply <= amountOut.Amount) {
				throw new Exception("Insufficient reserves");
			}

			if (!TryCalculateSwapAmountOfFixedOutputSwap(inputSupply, outputSupply, amountOut.Amount, out var swapAmount)) {
				throw new Exception("Invalid reserves");
			}

			var totalFeeAmount = CalculateFixedOutputFeeAmount(swapAmount);
			var assetInAmount = swapAmount + totalFeeAmount;
			var priceImpact = CalculatePriceImpact(inputSupply, outputSupply, assetInAmount, amountOut.Amount);
			var amountIn = new AssetAmount(assetIn, assetInAmount);
			var swapFees = new AssetAmount(amountIn.Asset, totalFeeAmount);

			var result = new SwapQuote() {
				SwapType = SwapType.FixedOutput,
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapFees = swapFees,
				Slippage = slippage,
				PriceImpact = priceImpact,
				LiquidityAsset = LiquidityAsset
			};

			return result;
		}

		protected virtual bool TryCalculateOutputAmountOfFixedInputSwap(
			ulong inputSupply, ulong outputSupply, ulong swapAmount, out ulong swapOutputAmount) {

			var k = BigInteger.Multiply(inputSupply, outputSupply);
			var inputSide = (ulong)BigInteger.Divide(k, BigInteger.Add(inputSupply, swapAmount)) + 1;

			if (outputSupply < inputSide) {
				swapOutputAmount = 0;
				return false;
			}

			swapOutputAmount = outputSupply - inputSide;

			return true;
		}

		protected virtual bool TryCalculateSwapAmountOfFixedOutputSwap(
			ulong inputSupply, ulong outputSupply, ulong outputAmount, out ulong swapAmount) {

			var k = BigInteger.Multiply(inputSupply, outputSupply);

			if (outputSupply <= outputAmount) {
				swapAmount = 0;
				return false;
			}

			var outputSide = k / (outputSupply - outputAmount);

			if (outputSide <= inputSupply) {
				swapAmount = 0;
				return false;
			}

			swapAmount = (ulong)outputSide - inputSupply + 1;
			return true;
		}

		protected virtual ulong CalculateFixedInputFeeAmount(ulong inputAmount) {
			return (ulong)BigInteger.Divide(
				BigInteger.Multiply(inputAmount, TotalFeeShare), 10_000);
		}

		protected virtual ulong CalculateFixedOutputFeeAmount(ulong swapAmount) {

			var inputAmount = (ulong)BigInteger.Divide(
				BigInteger.Multiply(swapAmount, 10_000),
				BigInteger.Subtract(10_000, TotalFeeShare));

			return inputAmount - swapAmount;
		}

		protected virtual ulong CalculateProtocolFeeAmount(ulong totalFeeAmount) {
			return totalFeeAmount / ProtocolFeeRatio;
		}

		protected virtual ulong CalculatePoolersFeeAmount(ulong totalFeeAmount) {

			var protocolFeeAmount = CalculateProtocolFeeAmount(totalFeeAmount);

			return totalFeeAmount - protocolFeeAmount;
		}

		protected static double CalculatePriceImpact(
			ulong inputSupply,
			ulong outputSupply,
			ulong swapInputAmount,
			ulong swapOutputAmount) {

			var swapPrice = (double)swapOutputAmount / (double)swapInputAmount;
			var poolPrice = (double)outputSupply / (double)inputSupply;
			var result = Math.Abs(Math.Round(((double)swapPrice / (double)poolPrice) - 1, 5));

			return result;
		}

	}

}
