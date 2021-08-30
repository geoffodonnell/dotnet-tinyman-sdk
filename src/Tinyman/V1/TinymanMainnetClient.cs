using Algorand.V2;
using System;

namespace Tinyman.V1 {

	public class TinymanMainnetClient : TinymanClient {

		public TinymanMainnetClient()
			: this (new AlgodApi(Constant.AlgodMainnetHost, String.Empty)) {
		}

		public TinymanMainnetClient(AlgodApi algodApi)
			: base(algodApi, Constant.MainnetValidatorAppId) {
		}
	}

}
