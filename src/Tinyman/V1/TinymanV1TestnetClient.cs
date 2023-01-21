using Algorand.Algod;
using System;
using System.Net.Http;

namespace Tinyman.V1 {

	/// <summary>
	/// Provides methods for interacting with Tinyman V1 AMM on Testnet via Algorand REST API.
	/// </summary>
	public class TinymanV1TestnetClient : TinymanV1Client {

		/// <summary>
		/// Construct a new instance
		/// </summary>
		public TinymanV1TestnetClient()
			: this(TinymanV1Constant.AlgodTestnetHost, String.Empty) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="defaultApi"></param>
		public TinymanV1TestnetClient(IDefaultApi defaultApi)
			: base(defaultApi, TinymanV1Constant.TestnetValidatorAppId) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="url"></param>
		public TinymanV1TestnetClient(HttpClient httpClient, string url)
			: base(httpClient, url, TinymanV1Constant.TestnetValidatorAppId) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="url"></param>
		/// <param name="token"></param>
		public TinymanV1TestnetClient(string url, string token)
			: base(url, token, TinymanV1Constant.TestnetValidatorAppId) { }

	}

}
