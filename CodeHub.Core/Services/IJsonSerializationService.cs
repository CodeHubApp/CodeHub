namespace CodeHub.Core.Services
{
    public interface IJsonSerializationService
    {
        string Serialize(object o);

        TData Deserialize<TData>(string data);
    }
}

