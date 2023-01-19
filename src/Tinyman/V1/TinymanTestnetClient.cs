using Algorand.Algod;
using System;
using System.Net.Http;

namespace Tinyman.V1 {

	public class TinymanTestnetClient : TinymanClient {

		public TinymanTestnetClient()
			: this(Constant.AlgodTestnetHost, String.Empty) { }

		public TinymanTestnetClient(IDefaultApi defaultApi)
			: base(defaultApi, Constant.TestnetValidatorAppId) { }

		public TinymanTestnetClient(HttpClient httpClient, string url)
			: base(httpClient, url, Constant.TestnetValidatorAppId) { }

		public TinymanTestnetClient(string url, string token)
			: base(url, token, Constant.TestnetValidatorAppId) { }

	}

}
