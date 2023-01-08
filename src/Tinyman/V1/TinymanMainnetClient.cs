﻿using Algorand.Algod;
using System;
using System.Net.Http;

namespace Tinyman.V1 {

	public class TinymanMainnetClient : TinymanClient {

		public TinymanMainnetClient()
			: this(Constant.AlgodMainnetHost, String.Empty) { }

		public TinymanMainnetClient(IDefaultApi defaultApi) 
			: base(defaultApi, Constant.MainnetValidatorAppId) { }

		public TinymanMainnetClient(HttpClient httpClient, string url)
			: base(httpClient, url, Constant.MainnetValidatorAppId) { }

		public TinymanMainnetClient(string url, string token)
			: base(url, token, Constant.MainnetValidatorAppId) { }

	}

}
