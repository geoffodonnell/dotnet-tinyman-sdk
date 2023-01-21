using Algorand.Algod;
using System;
using System.Net.Http;

namespace Tinyman.V2 {

	/// <summary>
	/// Provides methods for interacting with Tinyman V2 AMM on Testnet via Algorand REST API.
	/// </summary>
	public class TinymanV2TestnetClient : TinymanV2Client {

		/// <summary>
		/// Construct a new instance
		/// </summary>
		public TinymanV2TestnetClient()
			: this(TinymanV2Constant.AlgodTestnetHost, String.Empty) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="defaultApi"></param>
		public TinymanV2TestnetClient(IDefaultApi defaultApi)
			: base(defaultApi, TinymanV2Constant.TestnetValidatorAppIdV2_0) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="url"></param>
		public TinymanV2TestnetClient(HttpClient httpClient, string url)
			: base(httpClient, url, TinymanV2Constant.TestnetValidatorAppIdV2_0) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="url"></param>
		/// <param name="token"></param>
		public TinymanV2TestnetClient(string url, string token)
			: base(url, token, TinymanV2Constant.TestnetValidatorAppIdV2_0) { }

	}

}
