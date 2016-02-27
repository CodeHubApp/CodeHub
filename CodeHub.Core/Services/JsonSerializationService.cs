using System;
using CodeHub.Core.Services;

namespace CodeHub.Core.Services
{
    public class JsonSerializationService : IJsonSerializationService
    {
        private readonly GitHubSharp.SimpleJsonSerializer _serializer;

        public JsonSerializationService()
        {
            _serializer = new GitHubSharp.SimpleJsonSerializer();
        }

        public string Serialize(object o)
        {
            return _serializer.Serialize(o);
        }

        public TData Deserialize<TData>(string data)
        {
            return _serializer.Deserialize<TData>(data);
        }
    }
}

