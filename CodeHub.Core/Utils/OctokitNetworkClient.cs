using Octokit.Internal;
using Octokit;
using System.Threading.Tasks;
using CodeHub.Core.Services;

namespace CodeHub.Core.Utilities
{
    /// <summary>
    /// A decorator class for the <see cref="IHttpClient"/> object which will 
    /// trigger the network activity spinner
    /// </summary>
    class OctokitNetworkClient : IHttpClient
    {
        private readonly IHttpClient _httpClient;
        private readonly INetworkActivityService _networkActivity;

        public OctokitNetworkClient(IHttpClient httpClient, INetworkActivityService networkActivity)
        {
            _httpClient = httpClient;
            _networkActivity = networkActivity;
        }

        public async Task<IResponse> Send(IRequest request, System.Threading.CancellationToken cancellationToken)
        {
            using (_networkActivity.ActivateNetwork())
                return await _httpClient.Send(request, cancellationToken);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}

