using System;
using Octokit.Internal;
using Octokit;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Akavache;
using System.Reactive.Linq;

namespace CodeHub.Core.Utilities
{
    class OctokitCacheClient : IHttpClient
    {
        private readonly IHttpClient _httpClient;

        public OctokitCacheClient(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResponse<T>> Send<T>(IRequest request, System.Threading.CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
                return await _httpClient.Send<T>(request, cancellationToken);

            var key = "octokit_cache_" + new Uri(request.BaseAddress, request.Endpoint).AbsoluteUri;
            var response = await BlobCache.LocalMachine.GetObject<IResponse<T>>(key).Catch(Observable.Empty<IResponse<T>>()).DefaultIfEmpty();

            if (response == null)
            {
                response = await _httpClient.Send<T>(request, cancellationToken);
                await BlobCache.LocalMachine.InsertObject(key, response, TimeSpan.FromMinutes(30));
                return response;
            }

            if (!String.IsNullOrEmpty(response.ApiInfo.Etag))
            {
                request.Headers["If-None-Match"] = String.Format("{0}", response.ApiInfo.Etag);
                var conditionalResponse = await _httpClient.Send<T>(request, cancellationToken);
                if (conditionalResponse.StatusCode == HttpStatusCode.NotModified)
                    return response;

                await BlobCache.LocalMachine.InsertObject(key, conditionalResponse, TimeSpan.FromMinutes(30));
                return conditionalResponse;
            }

            response = await _httpClient.Send<T>(request, cancellationToken);
            await BlobCache.LocalMachine.InsertObject(key, response, TimeSpan.FromMinutes(30));
            return response;
        }
    }
}

