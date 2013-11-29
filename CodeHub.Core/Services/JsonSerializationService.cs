using System;
using CodeFramework.Core.Services;

namespace CodeHub.Core.Services
{
	public class JsonSerializationService : IJsonSerializationService
    {
		public string Serialize(object o)
		{
			return new RestSharp.Serializers.JsonSerializer().Serialize(o);
		}

		public TData Deserialize<TData>(string data)
		{
			return new RestSharp.Deserializers.JsonDeserializer().Deserialize<TData>(new RestSharp.RestResponse { Content = data });
		}
    }
}

