using CodeFramework.Core.Services;
using System.Text.RegularExpressions;

namespace CodeHub.Core.Services
{
	public class JsonSerializationService : IJsonSerializationService
    {
		private static readonly Newtonsoft.Json.JsonSerializerSettings Settings = new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new JsonLowerCaseUnderscoreContractResolver() };
		public T Deserialize<T>(string data)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data, Settings);
		}

		public string Serialize(object data)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.None, Settings);
		}

		public class JsonLowerCaseUnderscoreContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
		{
			private readonly Regex regex = new Regex("(?!(^[A-Z]))([A-Z])");

			protected override string ResolvePropertyName(string propertyName)
			{
				return regex.Replace(propertyName, "_$2").ToLower();
			}
		}
    }
}

