# dotnet-tinyman-sdk
[![Dev CI Status](https://dev.azure.com/gbo-devops/github-pipelines/_apis/build/status/Tinyman%20Dev%20CI?branchName=develop)](https://dev.azure.com/gbo-devops/github-pipelines/_build/latest?definitionId=1&branchName=develop)
[![NuGet version](https://badge.fury.io/nu/tinyman.svg)](https://badge.fury.io/nu/tinyman)
[![Donate Algo](https://img.shields.io/badge/Donate-ALGO-000000.svg?style=flat)](https://algoexplorer.io/address/EJMR773OGLFAJY5L2BCZKNA5PXLDJOWJK4ED4XDYTYH57CG3JMGQGI25DQ)

# Overview
This library provides access to the [Tinyman AMM](https://docs.tinyman.org/) on the Algorand blockchain.

## Known Issues

### :exclamation: High Severity :exclamation:
* The V1 pools should NOT be used as they are vulnerable to a liquidity draining attack.
	* See the announcement [here](https://tinymanorg.medium.com/official-announcement-about-the-incidents-of-01-01-2022-56abb19d8b19) for more information.

# Installation
Releases are available at [nuget.org](https://www.nuget.org/packages/Tinyman/).

## Package Manager
```
PM> Install-Package -Id Tinyman
```

## .NET CLI
```
dotnet add package Tinyman
```

# Usage

## Swapping
Swap one asset for another in an existing pool.

```C#
// Initialize the client
var client = new TinymanTestnetClient();

// Get the assets
var tinyUsdc = await client.FetchAssetAsync(21582668);
var algo = await client.FetchAssetAsync(0);

// Get the pool
var pool = await client.FetchPoolAsync(algo, tinyUsdc);

// Get a quote to swap 1 Algo for tinyUsdc
var amountIn = Algorand.Utils.AlgosToMicroalgos(1.0);
var quote = pool.CalculateFixedInputSwapQuote(new AssetAmount(algo, amountIn), 0.05);

// Convert to action
var action = Swap.FromQuote(quote);

// Perform the swap
var result = await client.SwapAsync(account, action);
```

## Minting
Add assets to an existing pool in exchange for the liquidity pool asset.

```C#
// Initialize the client
var client = new TinymanTestnetClient();

// Get the assets
var tinyUsdc = await client.FetchAssetAsync(21582668);
var algo = await client.FetchAssetAsync(0);

// Get the pool
var pool = await client.FetchPoolAsync(algo, tinyUsdc);

// Get a quote to add 1 Algo and the corresponding tinyUsdc amount to the pool
var amountIn = Algorand.Utils.AlgosToMicroalgos(1.0);
var quote = pool.CalculateMintQuote(new AssetAmount(algo, amountIn), 0.05);

// Convert to action
var action = Mint.FromQuote(quote);

// Perform the minting
var result = await client.MintAsync(account, action);
```

## Burning
Exchange the liquidity pool asset for the pool assets.

```C#
// Initialize the client
var client = new TinymanTestnetClient();

// Get the assets
var tinyUsdc = await client.FetchAssetAsync(21582668);
var algo = await client.FetchAssetAsync(0);

// Get the pool
var pool = await client.FetchPoolAsync(algo, tinyUsdc);

// Get a quote to swap the entire liquidity pool asset balance for pooled assets
var amount = client.GetBalance(account.Address, pool.LiquidityAsset);
var quote = pool.CalculateBurnQuote(amount, 0.05);

// Convert to action
var action = Burn.FromQuote(quote);

// Perform the burning
var result = await client.BurnAsync(account, action);
```

## Redeeming
Redeem excess amounts from previous transactions.

```C#
// Initialize the client
var client = new TinymanTestnetClient();

// Fetch the amounts
var excessAmounts = await client.FetchExcessAmountsAsync(account.Address);

// Redeem each amount
foreach (var quote in excessAmounts) {

	var action = Redeem.FromQuote(quote);
	var result = await client.RedeemAsync(account, action);
}
```

# Examples
Full examples, simple and verbose, can be found in [/example](/example).

# Build
dotnet-tinyman-sdk build pipelines use the [Assembly Info Task](https://github.com/BMuuN/vsts-assemblyinfo-task) extension.

# License
dotnet-tinyman-sdk is licensed under a MIT license except for the exceptions listed below. See the LICENSE file for details.

## Exceptions
`src\Tinyman\V1\asc.json` is currently unlicensed. It may be used by this SDK but may not be used in any other way or be distributed separately without the express permission of Tinyman. It is used here with permission.

# Disclaimer
Nothing in the repo constitutes professional and/or financial advice. Use this SDK at your own risk.