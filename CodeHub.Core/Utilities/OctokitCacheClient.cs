using System;
using Octokit.Internal;
using Octokit;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Akavache;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace CodeHub.Core.Utilities
{
    class OctokitCacheClient : IHttpClient
    {
        private readonly IHttpClient _httpClient;

        public OctokitCacheClient(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResponse> Send(IRequest request, System.Threading.CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
                return await _httpClient.Send(request, cancellationToken);

            var key = "octokit_cache_" + new Uri(request.BaseAddress, request.Endpoint).AbsoluteUri;
            var response = await BlobCache.LocalMachine.GetObject<IResponse>(key).Catch(Observable.Empty<IResponse>()).DefaultIfEmpty();

            if (response == null)
            {
                response = await _httpClient.Send(request, cancellationToken);
                await BlobCache.LocalMachine.InsertObject(key, response, TimeSpan.FromMinutes(30));
                return response;
            }

            if (!String.IsNullOrEmpty(response.ApiInfo.Etag))
            {
                request.Headers["If-None-Match"] = String.Format("{0}", response.ApiInfo.Etag);
                var conditionalResponse = await _httpClient.Send(request, cancellationToken);
                if (conditionalResponse.StatusCode == HttpStatusCode.NotModified)
                    return response;

                await BlobCache.LocalMachine.InsertObject(key, conditionalResponse, TimeSpan.FromMinutes(30));
                return conditionalResponse;
            }

            response = await _httpClient.Send(request, cancellationToken);
            await BlobCache.LocalMachine.InsertObject(key, response, TimeSpan.FromMinutes(30));
            return response;
        }

        private class InternalResponse
        {
            public object Body { get; private set; }
            public Dictionary<string, string> Headers { get; set; }
            public ApiInfo ApiInfo { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public string ContentType { get; set; }
        }
    }
}

