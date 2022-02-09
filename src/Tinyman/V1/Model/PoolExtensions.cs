using System;
using System.Numerics;

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

			var result = new SwapQuote(pool) {
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

			var result = new SwapQuote(pool) {
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

		public static BurnQuote CalculateBurnQuote(
			this Pool pool,
			AssetAmount amountIn,
			double slippage = 0.05) {

			if (pool.LiquidityAsset.Id != amountIn.Asset.Id) {
				throw new ArgumentException(
					$"Expected '{nameof(amountIn)}' to be liquidity pool asset amount.");
			}

			var asset1Amount = BigInteger.Divide(
				BigInteger.Multiply(amountIn.Amount, pool.Asset1Reserves), pool.IssuedLiquidity);
			var asset2Amount = BigInteger.Divide(
				BigInteger.Multiply(amountIn.Amount, pool.Asset2Reserves), pool.IssuedLiquidity);

			var result = new BurnQuote(pool) {
				AmountsOut = new Tuple<AssetAmount, AssetAmount>(
					new AssetAmount(pool.Asset1, (ulong)asset1Amount),
					new AssetAmount(pool.Asset2, (ulong)asset2Amount)),
				LiquidityAssetAmount = amountIn,
				Slippage = slippage
			};

			return result;
		}

		public static MintQuote CalculateMintQuote(
			this Pool pool,
			AssetAmount amount,
			double slippage = 0.05) {

			return CalculateMintQuote(
				pool, new Tuple<AssetAmount, AssetAmount>(amount, null), slippage);
		}

		public static MintQuote CalculateMintQuote(
			this Pool pool,
			Tuple<AssetAmount, AssetAmount> amounts,
			double slippage = 0.05) {

			var amount1 = default(AssetAmount);
			var amount2 = default(AssetAmount);

			if (amounts.Item1?.Asset == pool.Asset1) {
				amount1 = amounts.Item1;
				amount2 = amounts.Item2;
			} else {
				amount1 = amounts.Item2;
				amount2 = amounts.Item1;
			}

			var liquidityAssetAmount = 0ul;

			if (!pool.Exists) {
				throw new Exception("Pool has not been bootstrapped yet!");
			}

			// Pool exists and contains assets
			if (pool.IssuedLiquidity > 0) {

				if (amount1 == null) {
					amount1 = pool.Convert(amount2);
				}

				if (amount2 == null) {
					amount2 = pool.Convert(amount1);
				}

				liquidityAssetAmount = Math.Min(
					(ulong)BigInteger.Divide(BigInteger.Multiply(amount1.Amount, pool.IssuedLiquidity), pool.Asset1Reserves),
					(ulong)BigInteger.Divide(BigInteger.Multiply(amount2.Amount, pool.IssuedLiquidity), pool.Asset2Reserves));

			// Pool exists does not contain assets
			} else {

				if (amount1 == null || amount2 == null) {
					throw new Exception("Amounts required for both assets for first mint!");
				}

				liquidityAssetAmount = Convert.ToUInt64(
					Math.Sqrt((double)BigInteger.Multiply(amount1.Amount, amount2.Amount)) - 1000);
				slippage = 0;
			}

			var result = new MintQuote(pool) {
				AmountsIn = new Tuple<AssetAmount, AssetAmount>(amount1, amount2),
				LiquidityAssetAmount = new AssetAmount(pool.LiquidityAsset, liquidityAssetAmount),
				Slippage = slippage
			};

			return result;
		}

	}

}