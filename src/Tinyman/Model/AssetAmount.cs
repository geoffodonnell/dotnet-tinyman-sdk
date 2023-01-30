﻿using System;
using System.Collections.Generic;

namespace Tinyman.Model {

	/// <summary>
	/// Represents an amount of a specific asset.
	/// </summary>
	public class AssetAmount {

		private static string AssetMismatchMessage =
			"AssetAmount arithmetic operators are only valid for matching Assets.";

		/// <summary>
		/// The asset.
		/// </summary>
		public virtual Asset Asset { get; set; }

		/// <summary>
		/// The amount.
		/// </summary>
		public virtual ulong Amount { get; set; }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		public AssetAmount() { }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		/// <param name="asset">The asset</param>
		/// <param name="amount">The amount</param>
		public AssetAmount(Asset asset, ulong amount) {
			Asset = asset;
			Amount = amount;
		}

		public static bool operator >(AssetAmount a, AssetAmount b) {
			if (a.Asset.Id != b.Asset.Id) {
				throw new ArgumentException(AssetMismatchMessage);
			}

			return a.Amount > b.Amount;
		}

		public static bool operator >(AssetAmount a, ulong b) {
			return a.Amount > b;
		}

		public static bool operator <(AssetAmount a, AssetAmount b) {
			if (a.Asset.Id != b.Asset.Id) {
				throw new ArgumentException(AssetMismatchMessage);
			}

			return a.Amount < b.Amount;
		}

		public static bool operator <(AssetAmount a, ulong b) {
			return a.Amount < b;
		}

		public static bool operator ==(AssetAmount a, AssetAmount b) {
			if (a is null && b is null) {
				return true;
			}

			if (a is null || b is null) {
				return false;
			}

			if (a.Asset == null && b.Asset == null) {
				return a.Amount == b.Amount;
			}

			if (a.Asset?.Id != b.Asset?.Id) {
				throw new ArgumentException(AssetMismatchMessage);
			}

			return a.Amount == b.Amount;
		}

		public static bool operator ==(AssetAmount a, ulong b) {
			return a.Amount == b;
		}

		public static bool operator !=(AssetAmount a, AssetAmount b) {
			if (a is null && b is null) {
				return false;
			}

			if (a is null || b is null) {
				return true;
			}

			if (a.Asset == null && b.Asset == null) {
				return a.Amount != b.Amount;
			}

			if (a.Asset.Id != b.Asset.Id) {
				throw new ArgumentException(AssetMismatchMessage);
			}

			return a?.Amount != b?.Amount;
		}

		public static bool operator !=(AssetAmount a, ulong b) {
			return a?.Amount != b;
		}

		public static AssetAmount operator +(AssetAmount a, AssetAmount b) {
			if (a.Asset.Id != b.Asset.Id) {
				throw new ArgumentException(AssetMismatchMessage);
			}

			return new AssetAmount {
				Asset = a.Asset,
				Amount = a.Amount + b.Amount
			};
		}

		public static AssetAmount operator -(AssetAmount a, AssetAmount b) {
			if (a.Asset.Id != b.Asset.Id) {
				throw new ArgumentException(AssetMismatchMessage);
			}

			return new AssetAmount {
				Asset = a.Asset,
				Amount = a.Amount - b.Amount
			};
		}

		public static AssetAmount operator *(AssetAmount a, ulong b) {
			return new AssetAmount {
				Asset = a.Asset,
				Amount = a.Amount * b
			};
		}

		public static AssetAmount operator *(AssetAmount a, double b) {
			return new AssetAmount {
				Asset = a.Asset,
				Amount = Convert.ToUInt64(a.Amount * b)
			};
		}

		public static AssetAmount operator /(AssetAmount a, ulong b) {
			return new AssetAmount {
				Asset = a.Asset,
				Amount = a.Amount / b
			};
		}

		public static AssetAmount operator /(AssetAmount a, double b) {
			return new AssetAmount {
				Asset = a.Asset,
				Amount = Convert.ToUInt64(a.Amount / b)
			};
		}

		public override string ToString() {

			var amount = Amount / Math.Pow(10, Asset.Decimals);

			return $"{amount} {Asset.UnitName}";
		}

		public override bool Equals(object obj) {
			return obj?.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode() {
			var hashCode = -177531428;
			hashCode = hashCode * -1521134295 + EqualityComparer<Asset>.Default.GetHashCode(Asset);
			hashCode = hashCode * -1521134295 + Amount.GetHashCode();
			return hashCode;
		}

	}

}