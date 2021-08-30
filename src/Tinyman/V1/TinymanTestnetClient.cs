using Algorand.V2;
using System;

namespace Tinyman.V1 {

	public class TinymanTestnetClient: TinymanClient {

		public TinymanTestnetClient()
			: this (new AlgodApi(Constant.AlgodTestnetHost, String.Empty)) {
		}

		public TinymanTestnetClient(AlgodApi algodApi)
			: base(algodApi, Constant.TestnetValidatorAppId) {
		}
	}

}
