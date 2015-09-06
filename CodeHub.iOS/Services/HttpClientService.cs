using CodeFramework.Core.Services;
using System.Net.Http;

namespace CodeFramework.iOS.Services
{
	public class HttpClientService : IHttpClientService
    {
		public HttpClient Create()
		{
			return new HttpClient(); //new ModernHttpClient.AFNetworkHandler()
		}
    }
}

