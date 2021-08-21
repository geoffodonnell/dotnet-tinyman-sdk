using Algorand.V2;
using System;

namespace Tinyman.V1 {

	public class TinymanTestnetClient: TinymanClient {

		public TinymanTestnetClient()
			: this (new AlgodApi(Constants.AlgodTestnetHost, String.Empty)) {
		}

		public TinymanTestnetClient(AlgodApi algodApi)
			: base(algodApi, Constants.TESTNET_VALIDATOR_APP_ID) {
		}
	}

}
