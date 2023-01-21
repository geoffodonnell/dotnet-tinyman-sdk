using Algorand.Algod;
using System;
using System.Net.Http;

namespace Tinyman.V2 {

	/// <summary>
	/// Provides methods for interacting with Tinyman V2 AMM on Mainnet via Algorand REST API.
	/// </summary>
	public class TinymanV2MainnetClient : TinymanV2Client {

		/// <summary>
		/// Construct a new instance
		/// </summary>
		public TinymanV2MainnetClient()
			: this(TinymanV2Constant.AlgodMainnetHost, String.Empty) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="defaultApi"></param>
		public TinymanV2MainnetClient(IDefaultApi defaultApi) 
			: base(defaultApi, TinymanV2Constant.MainnetValidatorAppIdV2_0) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="url"></param>
		public TinymanV2MainnetClient(HttpClient httpClient, string url)
			: base(httpClient, url, TinymanV2Constant.MainnetValidatorAppIdV2_0) { }

		/// <summary>
		/// Construct a new instance
		/// </summary>
		/// <param name="url"></param>
		/// <param name="token"></param>
		public TinymanV2MainnetClient(string url, string token)
			: base(url, token, TinymanV2Constant.MainnetValidatorAppIdV2_0) { }

	}

}
