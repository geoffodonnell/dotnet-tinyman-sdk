﻿using System;
using System.Net.Http;

namespace Tinyman.V1 {

	public class TinymanTestnetClient: TinymanClient {

		public TinymanTestnetClient()
			: this(Constant.AlgodMainnetHost, String.Empty) { }

		public TinymanTestnetClient(HttpClient httpClient, string url)
			: base(httpClient, url, Constant.TestnetValidatorAppId) { }

		public TinymanTestnetClient(string url, string token)
			: base(url, token, Constant.TestnetValidatorAppId) { }

	}

}
