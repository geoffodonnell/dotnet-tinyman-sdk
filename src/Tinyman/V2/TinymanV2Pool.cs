using System;
using System.Numerics;
using Tinyman.Model;
using Tinyman.V2.Model;

namespace Tinyman.V2 {

	/// <summary>
	/// Represents an asset pool within the Tinyman V2 AMM
	/// </summary>
	public class TinymanV2Pool : Pool {

		/// <summary>
		/// Protocol fees of first pool asset
		/// </summary>
		public virtual ulong Asset1ProtocolFees { get; set; }

		/// <summary>
		/// Protocol fees of second pool asset
		/// </summary>
		public virtual ulong Asset2ProtocolFees { get; set; }

		/// <summary>
		/// Total fee share
		/// </summary>
		public virtual ulong TotalFeeShare { get; set; }

		/// <summary>
		/// Protocol fee ratio
		/// </summary>
		public virtual ulong ProtocolFeeRatio { get; set; }

		/// <inheritdoc/>
		public TinymanV2Pool() { }

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public override SwapQuote CalculateFixedInputSwapQuote(
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

			var result = new SwapQuote {
				SwapType = SwapType.FixedInput,
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapFees = swapFees,
				Slippage = slippage,
				PriceImpact = priceImpact,
				LiquidityAsset = LiquidityAsset,
				ValidatorApplicationId = ValidatorAppId
			};

			return result;
		}

		/// <inheritdoc/>
		public override SwapQuote CalculateFixedOutputSwapQuote(
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

			var result = new SwapQuote {
				SwapType = SwapType.FixedOutput,
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapFees = swapFees,
				Slippage = slippage,
				PriceImpact = priceImpact,
				LiquidityAsset = LiquidityAsset,
				ValidatorApplicationId = ValidatorAppId
			};

			return result;
		}

		/// <inheritdoc/>
		public override BurnQuote CalculateBurnQuote(
			AssetAmount amountIn,
			double slippage = 0.005) {

			if (LiquidityAsset.Id != amountIn.Asset.Id) {
				throw new ArgumentException(
					$"Expected '{nameof(amountIn)}' to be liquidity pool asset amount.");
			}

			ulong asset1Amount;
			ulong asset2Amount;

			if (IssuedLiquidity > (amountIn.Amount + TinymanV2Constant.LockedPoolTokens)) {
				asset1Amount = (ulong)BigInteger.Divide(
					BigInteger.Multiply(amountIn.Amount, Asset1Reserves), IssuedLiquidity);
				asset2Amount = (ulong)BigInteger.Divide(
					BigInteger.Multiply(amountIn.Amount, Asset2Reserves), IssuedLiquidity);
			} else {
				asset1Amount = Asset1Reserves;
				asset2Amount = Asset2Reserves;
			}

			var result = new BurnQuote {
				AmountsOut = new Tuple<AssetAmount, AssetAmount>(
					new AssetAmount(Asset1, asset1Amount),
					new AssetAmount(Asset2, asset2Amount)),
				LiquidityAssetAmount = amountIn,
				Slippage = slippage,
				ValidatorApplicationId = ValidatorAppId
			};

			return result;
		}

		/// <summary>
		/// Calculate a single asset burn quote given a liquidity asset amount
		/// </summary>
		/// <param name="amountIn">Liquidity asset amount</param>
		/// <param name="assetOut">Desired asset out</param>
		/// <param name="slippage">Slippage</param>
		/// <returns>Burn quote</returns>
		/// <exception cref="ArgumentException"></exception>
		public virtual SingleAssetBurnQuote CalculateSingleAssetBurnQuote(
			AssetAmount amountIn,
			Asset assetOut,
			double slippage = 0.005) {

			if (LiquidityAsset.Id != amountIn.Asset.Id) {
				throw new ArgumentException(
					$"Expected '{nameof(amountIn)}' to be liquidity pool asset amount.");
			}

			// Step 1 - Calculate a proportional burn quote
			var burnQuote = CalculateBurnQuote(amountIn, slippage);

			// Step 2 - Calculate a fixed input swap quote adjusting the reserves for the proportional burn
			ulong inputSupply;
			ulong outputSupply;

			if (assetOut == Asset1) {
				inputSupply = Asset2Reserves - burnQuote.AmountsOut.ForAsset(Asset2).Amount;
				outputSupply = Asset1Reserves - burnQuote.AmountsOut.ForAsset(Asset1).Amount;
			} else {
				inputSupply = Asset1Reserves - burnQuote.AmountsOut.ForAsset(Asset1).Amount;
				outputSupply = Asset2Reserves - burnQuote.AmountsOut.ForAsset(Asset2).Amount;
			}

			var swapIn = burnQuote.AmountsOut.ForOtherAsset(assetOut);
			var totalFeeAmount = CalculateFixedInputFeeAmount(swapIn.Amount);
			var swapAmount = swapIn.Amount - totalFeeAmount;

			if (!TryCalculateOutputAmountOfFixedInputSwap(inputSupply, outputSupply, swapAmount, out var assetOutAmount)) {
				throw new Exception("Insufficient reserves");
			}

			var priceImpact = CalculatePriceImpact(inputSupply, outputSupply, swapIn.Amount, assetOutAmount);
			var amountOut = new AssetAmount(assetOut, assetOutAmount);
			var swapFees = new AssetAmount(swapIn.Asset, totalFeeAmount);

			var swapQuote = new SwapQuote {
				SwapType = SwapType.FixedInput,
				AmountIn = swapIn,
				AmountOut = amountOut,
				SwapFees = swapFees,
				Slippage = slippage,
				PriceImpact = priceImpact,
				LiquidityAsset = LiquidityAsset,
				ValidatorApplicationId = ValidatorAppId
			};

			return new SingleAssetBurnQuote {
				AmountOut = burnQuote.AmountsOut.ForAsset(assetOut) + swapQuote.AmountOut,
				LiquidityAssetAmount = amountIn,
				Slippage = slippage,
				ValidatorApplicationId = ValidatorAppId,
				SwapQuote = swapQuote
			};
		}

		/// <inheritdoc/>
		public override MintQuote CalculateMintQuote(
			AssetAmount amount,
			double slippage = 0.005) {

			return CalculateMintQuote(
				new Tuple<AssetAmount, AssetAmount>(amount, null), slippage);
		}

		/// <inheritdoc/>
		public override MintQuote CalculateMintQuote(
			Tuple<AssetAmount, AssetAmount> amounts,
			double slippage = 0.005) {

			if (!Exists) {
				throw new Exception("Pool has not been bootstrapped yet!");
			}

			var amount1 = default(AssetAmount);
			var amount2 = default(AssetAmount);

			if (amounts.Item1?.Asset == Asset1) {
				amount1 = amounts.Item1;
				amount2 = amounts.Item2;
			} else {
				amount1 = amounts.Item2;
				amount2 = amounts.Item1;
			}

			var liquidityAssetAmount = 0ul;

			// Pool exists and contains assets
			if (IssuedLiquidity > 0) {

				if (amount1 != null && amount2 != null) {
					throw new ArgumentException(
						$"Initial liquidity has been provided. Use {nameof(CalculateFlexibleMintQuote)} instead.");
				} else if (amount1 == null) {
					amount1 = Convert(amount2);
				} else if (amount2 == null) {
					amount2 = Convert(amount1);
				}

				liquidityAssetAmount = CalculateLiquidityAssetAmount(amount1.Amount, amount2.Amount);

				// Pool exists does not contain assets
			} else {

				if (amount1 == null || amount2 == null) {
					throw new Exception("Amounts required for both assets for first mint!");
				}

				liquidityAssetAmount = System.Convert.ToUInt64(
					Math.Sqrt((double)BigInteger.Multiply(amount1.Amount, amount2.Amount)) - TinymanV2Constant.LockedPoolTokens);
				slippage = 0;
			}

			var result = new MintQuote {
				AmountsIn = new Tuple<AssetAmount, AssetAmount>(amount1, amount2),
				LiquidityAssetAmount = new AssetAmount(LiquidityAsset, liquidityAssetAmount),
				Slippage = slippage,
				PriceImpact = 0.0d,
				ValidatorApplicationId = ValidatorAppId
			};

			return result;
		}

		/// <summary>
		/// Calculate a single asset mint quote given a pool asset amount
		/// </summary>
		/// <param name="amount">Pool asset amount</param>
		/// <param name="slippage">Slippage</param>
		/// <returns>Mint quote</returns>
		public virtual SingleAssetMintQuote CalculateSingleAssetMintQuote(
			AssetAmount amount,
			double slippage = 0.005) {

			var otherAsset = Asset1 == amount.Asset ? Asset2 : Asset1;
			var otherAmount = new AssetAmount(otherAsset, 0);

			var flexibleQuote = CalculateFlexibleMintQuote(
				new Tuple<AssetAmount, AssetAmount>(amount, otherAmount), slippage);

			return new SingleAssetMintQuote {
				AmountIn = amount,
				Slippage = slippage,
				LiquidityAssetAmount = flexibleQuote.LiquidityAssetAmount,
				PriceImpact = flexibleQuote.PriceImpact,
				SwapQuote = flexibleQuote.SwapQuote,
				ValidatorApplicationId = ValidatorAppId
			};
		}

		public virtual FlexibleMintQuote CalculateFlexibleMintQuote(
			Tuple<AssetAmount, AssetAmount> amounts,
			double slippage = 0.005) {

			if (!Exists) {
				throw new Exception("Pool has not been bootstrapped yet!");
			}

			if (IssuedLiquidity == 0) {
				throw new ArgumentException(
					$"Initial liquidity has not been provided. Use {nameof(CalculateMintQuote)} instead.");
			}

			var amount1 = default(AssetAmount);
			var amount2 = default(AssetAmount);

			if (amounts.Item1?.Asset == Asset1) {
				amount1 = amounts.Item1;
				amount2 = amounts.Item2;
			} else {
				amount1 = amounts.Item2;
				amount2 = amounts.Item1;
			}

			var liquidityAssetAmount = 0ul;

			var oldK = BigInteger.Multiply(Asset1Reserves, Asset2Reserves);
			var newAsset1Reserves = Asset1Reserves + amount1.Amount;
			var newAsset2Reserves = Asset2Reserves + amount2.Amount;
			var newK = BigInteger.Multiply(newAsset1Reserves, newAsset2Reserves);
			var newIssuedLiquidity = (ulong)
				Math.Sqrt(
					(double)BigInteger.Divide(
						BigInteger.Multiply(newK, BigInteger.Pow(IssuedLiquidity, 2)),
						oldK));

			liquidityAssetAmount = newIssuedLiquidity - IssuedLiquidity;

			var calculatedAsset1Amount = (long)BigInteger.Divide(
				BigInteger.Multiply(liquidityAssetAmount, newAsset1Reserves), newIssuedLiquidity);

			var calculatedAsset2Amount = (long)BigInteger.Divide(
				BigInteger.Multiply(liquidityAssetAmount, newAsset2Reserves), newIssuedLiquidity);

			var asset1SwapAmount = (long)amount1.Amount - calculatedAsset1Amount;
			var asset2SwapAmount = (long)amount2.Amount - calculatedAsset2Amount;

			/* Swap not required */
			if (asset1SwapAmount >= 0 && asset2SwapAmount >= 0) {
				return new FlexibleMintQuote {
					AmountsIn = new Tuple<AssetAmount, AssetAmount>(
						new AssetAmount(Asset1, amount1.Amount),
						new AssetAmount(Asset2, amount2.Amount)),
					LiquidityAssetAmount = new AssetAmount(LiquidityAsset, liquidityAssetAmount),
					Slippage = slippage,
					PriceImpact = 0.0,
					SwapQuote = null,
					ValidatorApplicationId = ValidatorAppId
				};
			}

			var swapFromAsset1ToAsset2 = false;
			var swapInAmountWithoutFee = 0ul;
			var swapInAmount = 0ul;
			var swapOutAmount = 0ul;
			var swapTotalFeeAmount = 0ul;
			var feeAsPoolTokens = 0ul;

			if (asset1SwapAmount > asset2SwapAmount) {
				swapFromAsset1ToAsset2 = true;
				swapInAmountWithoutFee = (ulong)asset1SwapAmount;
				swapOutAmount = (ulong)Math.Abs(Math.Min(asset2SwapAmount, 0));
				swapTotalFeeAmount = CalculateInternalSwapFeeAmount(swapInAmountWithoutFee);
				feeAsPoolTokens = (ulong)BigInteger.Divide(
					BigInteger.Multiply(swapTotalFeeAmount, newIssuedLiquidity),
					BigInteger.Multiply(newAsset1Reserves, 2));
				swapInAmount = swapInAmountWithoutFee + swapTotalFeeAmount;
				liquidityAssetAmount = liquidityAssetAmount - feeAsPoolTokens;
			} else {
				swapInAmountWithoutFee = (ulong)asset2SwapAmount;
				swapOutAmount = (ulong)Math.Abs(Math.Min(asset1SwapAmount, 0));
				swapTotalFeeAmount = CalculateInternalSwapFeeAmount(swapInAmountWithoutFee);
				feeAsPoolTokens = (ulong)BigInteger.Divide(
					BigInteger.Multiply(swapTotalFeeAmount, newIssuedLiquidity),
					BigInteger.Multiply(newAsset2Reserves, 2));
				swapInAmount = swapInAmountWithoutFee + swapTotalFeeAmount;
				liquidityAssetAmount = liquidityAssetAmount - feeAsPoolTokens;
			}

			var inputSupply = swapFromAsset1ToAsset2 ? Asset1Reserves : Asset2Reserves;
			var outputSupply = swapFromAsset1ToAsset2 ? Asset2Reserves : Asset1Reserves;
			var priceImpact = CalculatePriceImpact(inputSupply, outputSupply, swapInAmount, swapOutAmount);
			var swapInAsset = swapFromAsset1ToAsset2 ? Asset1 : Asset2;
			var swapOutAsset = swapFromAsset1ToAsset2 ? Asset2 : Asset1;

			return new FlexibleMintQuote {
				AmountsIn = new Tuple<AssetAmount, AssetAmount>(
						new AssetAmount(Asset1, amount1.Amount),
						new AssetAmount(Asset2, amount2.Amount)),
				LiquidityAssetAmount = new AssetAmount(LiquidityAsset, liquidityAssetAmount),
				Slippage = slippage,
				PriceImpact = priceImpact,
				SwapQuote = new SwapQuote {
					SwapType = SwapType.FixedInput,
					AmountIn = new AssetAmount(swapInAsset, swapInAmount),
					AmountOut = new AssetAmount(swapOutAsset, swapOutAmount),
					SwapFees = new AssetAmount(swapInAsset, swapTotalFeeAmount),
					Slippage = slippage,
					PriceImpact = priceImpact,
					LiquidityAsset = LiquidityAsset,
					ValidatorApplicationId = ValidatorAppId
				},
				ValidatorApplicationId = ValidatorAppId
			};
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

		protected virtual ulong CalculateInternalSwapFeeAmount(ulong swapAmount) {
			return (ulong)BigInteger.Divide(
				BigInteger.Multiply(swapAmount, TotalFeeShare), 
				BigInteger.Subtract(10_000, TotalFeeShare));
		}

		protected virtual ulong CalculateLiquidityAssetAmount(ulong amount1, ulong amount2) {

			var oldK = BigInteger.Multiply(Asset1Reserves, Asset2Reserves);
			var newAsset1Reserves = Asset1Reserves + amount1;
			var newAsset2Reserves = Asset2Reserves + amount2;
			var newK = BigInteger.Multiply(newAsset1Reserves, newAsset2Reserves);
			var newIssuedLiquidity = (ulong)Math.Sqrt((double)BigInteger.Divide(
				BigInteger.Multiply(newK, BigInteger.Pow(IssuedLiquidity, 2)), oldK));

			return newIssuedLiquidity - IssuedLiquidity;
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
