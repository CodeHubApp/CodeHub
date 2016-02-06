using CodeHub.Core.Services;
using System.Net.Http;

namespace CodeHub.iOS.Services
{
	public class HttpClientService : IHttpClientService
    {
		public HttpClient Create()
		{
			return new HttpClient(); //new ModernHttpClient.AFNetworkHandler()
		}
    }
}

