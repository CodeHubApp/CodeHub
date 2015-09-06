using System.Threading.Tasks;

namespace CodeFramework.Core.Services
{
    public interface IJsonHttpClientService
    {
        Task<TMessage> Get<TMessage>(string url);
    }
}

