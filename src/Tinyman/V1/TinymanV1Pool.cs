using System;
using System.Numerics;
using Tinyman.Model;

namespace Tinyman.V1 {

	/// <summary>
	/// Represents an asset pool within the Tinyman V1 AMM
	/// </summary>
	public class TinymanV1Pool : TinymanPool {

		/// <summary>
		/// Unclaimed protocol fees
		/// </summary>
		public virtual ulong UnclaimedProtocolFees { get; set; }

		/// <summary>
		/// Outstanding amount of first pool asset
		/// </summary>
		public virtual ulong OutstandingAsset1Amount { get; set; }

		/// <summary>
		/// Outstanding amount of second pool asset
		/// </summary>
		public virtual ulong OutstandingAsset2Amount { get; set; }

		/// <summary>
		/// Outstanding amount of liquidity asset
		/// </summary>
		public virtual ulong OutstandingLiquidityAssetAmount { get; set; }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		public TinymanV1Pool() { }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		/// <param name="asset1">Pool asset</param>
		/// <param name="asset2">Pool asset</param>
		internal TinymanV1Pool(Asset asset1, Asset asset2) {

			if (asset1.Id > asset2.Id) {
				Asset1 = asset1;
				Asset2 = asset2;
			} else {
				Asset1 = asset2;
				Asset2 = asset1;
			}

			Exists = false;
		}

		/// <inheritdoc />
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

			// k = input_supply * output_supply
			// ignoring fees, k must remain constant 
			// (input_supply + asset_in) * (output_supply - amount_out) = k
			var k = BigInteger.Multiply(inputSupply, outputSupply);
			var assetInAmountMinusFee = System.Convert.ToUInt64(amountIn.Amount * 997d / 1000d);
			var swapFees = amountIn.Amount - assetInAmountMinusFee;
			var assetOutAmount = (ulong)(outputSupply - k / (inputSupply + assetInAmountMinusFee));

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
				Slippage = slippage,
				LiquidityAsset = LiquidityAsset,
				ValidatorApplicationId = ValidatorAppId
			};

			return result;
		}

		/// <inheritdoc />
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

			// k = input_supply * output_supply
			// ignoring fees, k must remain constant 
			// (input_supply + asset_in) * (output_supply - amount_out) = k
			var k = BigInteger.Multiply(inputSupply, outputSupply);
			var calculatedAmountInWithoutFee = (ulong)(k / (outputSupply - amountOut.Amount) - inputSupply);
			var assetInAmount = calculatedAmountInWithoutFee * 1000d / 997d;
			var swapFees = assetInAmount - calculatedAmountInWithoutFee;

			var amountIn = new AssetAmount {
				Asset = assetIn,
				Amount = System.Convert.ToUInt64(assetInAmount)
			};

			var result = new SwapQuote {
				SwapType = SwapType.FixedOutput,
				AmountIn = amountIn,
				AmountOut = amountOut,
				SwapFees = new AssetAmount {
					Asset = amountIn.Asset,
					Amount = System.Convert.ToUInt64(swapFees)
				},
				Slippage = slippage,
				LiquidityAsset = LiquidityAsset,
				ValidatorApplicationId = ValidatorAppId
			};

			return result;
		}

		/// <inheritdoc />
		public override BurnQuote CalculateBurnQuote(
			AssetAmount amountIn,
			double slippage = 0.005) {

			if (LiquidityAsset.Id != amountIn.Asset.Id) {
				throw new ArgumentException(
					$"Expected '{nameof(amountIn)}' to be liquidity pool asset amount.");
			}

			var asset1Amount = BigInteger.Divide(
				BigInteger.Multiply(amountIn.Amount, Asset1Reserves), IssuedLiquidity);
			var asset2Amount = BigInteger.Divide(
				BigInteger.Multiply(amountIn.Amount, Asset2Reserves), IssuedLiquidity);

			var result = new BurnQuote {
				AmountsOut = new Tuple<AssetAmount, AssetAmount>(
					new AssetAmount(Asset1, (ulong)asset1Amount),
					new AssetAmount(Asset2, (ulong)asset2Amount)),
				LiquidityAssetAmount = amountIn,
				Slippage = slippage,
				ValidatorApplicationId = ValidatorAppId
			};

			return result;
		}

		/// <inheritdoc />
		public override MintQuote CalculateMintQuote(
			AssetAmount amount,
			double slippage = 0.005) {

			return CalculateMintQuote(
				new Tuple<AssetAmount, AssetAmount>(amount, null), slippage);
		}

		/// <inheritdoc />
		public override MintQuote CalculateMintQuote(
			Tuple<AssetAmount, AssetAmount> amounts,
			double slippage = 0.005) {

			var amount1 = default(AssetAmount);
			var amount2 = default(AssetAmount);

			if (amounts.Item1?.Asset == Asset1) {
				amount1 = amounts.Item1;
				amount2 = amounts.Item2;
			} else {
				amount1 = amounts.Item2;
				amount2 = amounts.Item1;
			}

			var hasExistingLiquidity = IssuedLiquidity > 0;
			var liquidityAssetAmount = 0ul;

			if (!Exists) {
				throw new Exception("Pool has not been bootstrapped yet!");
			}

			// Pool exists and contains assets
			if (hasExistingLiquidity) {

				if (amount1 == null) {
					amount1 = Convert(amount2);
				}

				if (amount2 == null) {
					amount2 = Convert(amount1);
				}

				liquidityAssetAmount = Math.Min(
					(ulong)BigInteger.Divide(BigInteger.Multiply(amount1.Amount, IssuedLiquidity), Asset1Reserves),
					(ulong)BigInteger.Divide(BigInteger.Multiply(amount2.Amount, IssuedLiquidity), Asset2Reserves));

				// Pool exists does not contain assets
			} else {

				if (amount1 == null || amount2 == null) {
					throw new Exception("Amounts required for both assets for first mint!");
				}

				liquidityAssetAmount = System.Convert.ToUInt64(
					Math.Sqrt((double)BigInteger.Multiply(amount1.Amount, amount2.Amount)) - 1000);
				slippage = 0;
			}

			var result = new MintQuote {
				AmountsIn = new Tuple<AssetAmount, AssetAmount>(amount1, amount2),
				LiquidityAssetAmount = new AssetAmount(LiquidityAsset, liquidityAssetAmount),
				Slippage = slippage,
				ValidatorApplicationId = ValidatorAppId,
				IsInitialLiquidity = !hasExistingLiquidity
			};

			return result;
		}

	}

}
