﻿using System;

namespace Tinyman.V1.Model {

	public class AssetAmount {

		private static string AssetMismatchMessage =
			"AssetAmount arithmetic operators are only valid for matching Assets.";

		public Asset Asset { get; internal set; }

		public ulong Amount { get; internal set; }

		public AssetAmount() { }

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
			if (a.Asset.Id != b.Asset.Id) {
				throw new ArgumentException(AssetMismatchMessage);
			}

			return a.Amount == b.Amount;
		}

		public static bool operator ==(AssetAmount a, ulong b) {
			return a.Amount == b;
		}
		
		public static bool operator !=(AssetAmount a, AssetAmount b) {
			if (a.Asset.Id != b.Asset.Id) {
				throw new ArgumentException(AssetMismatchMessage);
			}

			return a.Amount != b.Amount;
		}

		public static bool operator !=(AssetAmount a, ulong b) {
			return a.Amount != b;
		}

		public static AssetAmount operator +(AssetAmount a, AssetAmount b) {
			if(a.Asset.Id != b.Asset.Id) {
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

			var amount = Amount / (Math.Pow(10, Asset.Decimals));

			return $"{amount} {Asset.UnitName}";
		}

		public override bool Equals(object obj) {

			if (obj is AssetAmount b){
				return Asset.Id == b.Asset.Id && Amount == b.Amount;
			}

			return false;
		}

		public override int GetHashCode() {
			return Asset.Id.GetHashCode() + Amount.GetHashCode();
		}

	}

}
