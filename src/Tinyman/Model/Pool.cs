using System;

namespace Tinyman.Model {

	/// <summary>
	/// Common properties and methods of Tinyman asset pools
	/// </summary>
	public abstract class Pool {

		/// <summary>
		/// Whether or not the pool exists
		/// </summary>
		public virtual bool Exists { get; set; }

		/// <summary>
		/// The network address of the pool
		/// </summary>
		public virtual string Address { get; set; }

		/// <summary>
		/// First pool asset
		/// </summary>
		public virtual Asset Asset1 { get; set; }

		/// <summary>
		/// Second pool asset
		/// </summary>
		public virtual Asset Asset2 { get; set; }

		/// <summary>
		/// Pool liquidity asset
		/// </summary>
		public virtual Asset LiquidityAsset { get; set; }

		/// <summary>
		/// First pool asset reserves
		/// </summary>
		public virtual ulong Asset1Reserves { get; set; }

		/// <summary>
		/// Second pool asset reservces
		/// </summary>
		public virtual ulong Asset2Reserves { get; set; }

		/// <summary>
		/// Total issued liquidity asset amount
		/// </summary>
		public virtual ulong IssuedLiquidity { get; set; }

		/// <summary>
		/// Validator application ID
		/// </summary>
		public virtual ulong ValidatorAppId { get; set; }

		/// <summary>
		/// Pool address $ALGO balance
		/// </summary>
		public virtual ulong AlgoBalance { get; set; }

		/// <summary>
		/// Round
		/// </summary>
		public virtual ulong Round { get; set; }

		/// <summary>
		/// Price of first pool asset
		/// </summary>
		public virtual double Asset1Price { get => (double)Asset2Reserves / (double)Asset1Reserves; }

		/// <summary>
		/// Price of second pool asset
		/// </summary>
		public virtual double Asset2Price { get => (double)Asset1Reserves / (double)Asset2Reserves; }

		/// <summary>
		/// Convert one pool asset into another
		/// </summary>
		/// <param name="amount">Amount</param>
		/// <returns>Conversion amount</returns>
		public virtual AssetAmount Convert(AssetAmount amount) {

			if (amount.Asset == Asset1) {
				return new AssetAmount(Asset2, (ulong)((double)amount.Amount * Asset1Price));
			}

			if (amount.Asset == Asset2) {
				return new AssetAmount(Asset1, (ulong)((double)amount.Amount * Asset2Price));
			}

			return null;
		}

		/// <summary>
		/// Calculate a swap quote given a fixed input
		/// </summary>
		/// <param name="amountIn">Amount in</param>
		/// <param name="slippage">Slippage</param>
		/// <returns>Swap quote</returns>
		/// <exception cref="Exception"></exception>
		public abstract SwapQuote CalculateFixedInputSwapQuote(AssetAmount amountIn, double slippage = 0.005);

		/// <summary>
		/// Calculate a swap quote given a fixed output
		/// </summary>
		/// <param name="amountOut">Amount out</param>
		/// <param name="slippage">Slippage</param>
		/// <returns>Swap quote</returns>
		/// <exception cref="Exception"></exception>
		public abstract SwapQuote CalculateFixedOutputSwapQuote(AssetAmount amountOut, double slippage = 0.005);

		/// <summary>
		/// Calculate a burn quote given a liquidity asset amount
		/// </summary>
		/// <param name="amountIn">Liquidity asset amount</param>
		/// <param name="slippage">Slippage</param>
		/// <returns>Burn quote</returns>
		/// <exception cref="ArgumentException"></exception>
		public abstract BurnQuote CalculateBurnQuote(AssetAmount amountIn, double slippage = 0.005);

		/// <summary>
		/// Calculate a mint quote given an amount in. Use this method on bootstrapped pools to
		/// provide additional liquidity.
		/// </summary>
		/// <remarks>The resulting quote will indicate the required amount of the corresponding pool asset and resulting liquidity asset.</remarks>
		/// <param name="amount">Amount in</param>
		/// <param name="slippage">Slippage</param>
		/// <returns>Mint quote</returns>
		public abstract MintQuote CalculateMintQuote(AssetAmount amount, double slippage = 0.005);

		/// <summary>
		/// Calculate a mint quote given an amount in. Use this method on bootstrapped pools to
		/// provide initial liquidity.
		/// </summary>
		/// <remarks>The resulting quote will indicate the resulting liquidity asset.</remarks>
		/// <param name="amounts">Amounts in</param>
		/// <param name="slippage">Slippage</param>
		/// <returns>Mint quote</returns>
		/// <exception cref="Exception"></exception>
		public abstract MintQuote CalculateMintQuote(Tuple<AssetAmount, AssetAmount> amounts, double slippage = 0.005);

	}

}