using Algorand;
using Algorand.Algod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Tinyman.IntegrationTestConsole {

	//SEE: https://github.com/FrankSzendzielarz/dotnet-algorand-sdk/issues/11
	internal class ByteArrayConverter : JsonConverter {

		public static readonly ByteArrayConverter Instance = new ByteArrayConverter();	

		public static void Attach(IDefaultApi defaultApi) {

			var settings = typeof(DefaultApi)
				.GetProperty("JsonSerializerSettings", BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(defaultApi) as JsonSerializerSettings;
			settings.Converters.Add(Instance);
		}

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(byte[]);
		}

		public override object ReadJson(
			JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

			var value = JToken.Load(reader);

			if (value == null || value.Type == JTokenType.Null) {
				return null;
			}

			/* Expect a string; throw away non-string values */
			if (value.Type != JTokenType.String) {
				return null;
			}

			try {
				return Convert.FromBase64String(value.Value<string>());
			} catch (FormatException) {
				/* Hack around the issue, which is addressed properly in the following PR:
				 * https://github.com/FrankSzendzielarz/dotnet-algorand-sdk/pull/13 */				 
				return (new Address(value.Value<string>())).Bytes;
			}
		}

		public override void WriteJson(
			JsonWriter writer, object value, JsonSerializer serializer) {

			throw new NotImplementedException();
		}

	}

}