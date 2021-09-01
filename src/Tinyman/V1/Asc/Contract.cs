using Newtonsoft.Json;

namespace Tinyman.V1.Asc {

	[JsonConverter(typeof(ContractConverter))]
	internal abstract class Contract {

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

	}

}
