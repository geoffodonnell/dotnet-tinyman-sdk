using Algorand.Algod;
using System;
using System.Net.Http;

namespace Tinyman.V1 {

	/// <summary>
	/// Provides methods for interacting with Tinyman V1 AMM on Mainnet via Algorand REST API.
	/// </summary>
	public class TinymanV1MainnetClient : TinymanV1Client {

		/// <summary>
		/// Construct a new instance
		/// </summary>
		public TinymanV1MainnetClient()
			: this(TinymanV1Constant.AlgodMainnetHost, String.Empty) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="defaultApi"></param>
		public TinymanV1MainnetClient(IDefaultApi defaultApi) 
			: base(defaultApi, TinymanV1Constant.MainnetValidatorAppId) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="url"></param>
		public TinymanV1MainnetClient(HttpClient httpClient, string url)
			: base(httpClient, url, TinymanV1Constant.MainnetValidatorAppId) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="url"></param>
		/// <param name="token"></param>
		public TinymanV1MainnetClient(string url, string token)
			: base(url, token, TinymanV1Constant.MainnetValidatorAppId) { }

	}

}
