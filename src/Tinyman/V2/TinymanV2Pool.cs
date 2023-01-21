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

	}

}
