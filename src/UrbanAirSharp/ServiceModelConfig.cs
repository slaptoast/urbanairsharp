using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UrbanAirSharp
{
	public static class ServiceModelConfig
	{
		public static readonly string Host = "https://go.urbanairship.com/";
		public static readonly HttpClient HttpClient = new HttpClient();
		public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

		public static void Create(string uaAppKey, string uaAppMAsterSecret)
		{
			var auth = string.Format("{0}:{1}", uaAppKey, uaAppMAsterSecret);
			auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));

			SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
			SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			SerializerSettings.DateParseHandling = DateParseHandling.DateTime;
			SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			SerializerSettings.DateFormatString = "yyyy-MM-ddTH:mm:ss";

			HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
			HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/vnd.urbanairship+json; version=3;");
		}
	}
}
