using System;
using Octokit.Internal;
using Octokit;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Akavache;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
            var cachedResponse = await BlobCache.LocalMachine.GetObject<InternalResponse>(key).Catch(Observable.Empty<InternalResponse>()).DefaultIfEmpty();

            if (cachedResponse == null)
            {
                var httpResponse = await _httpClient.Send(request, cancellationToken);
                cachedResponse = new InternalResponse(httpResponse);
                await BlobCache.LocalMachine.InsertObject(key, cachedResponse, TimeSpan.FromMinutes(30));
                return httpResponse;
            }

            if (!String.IsNullOrEmpty(cachedResponse.ApiInfo.Etag))
            {
                request.Headers["If-None-Match"] = String.Format("{0}", cachedResponse.ApiInfo.Etag);
                var conditionalResponse = await _httpClient.Send(request, cancellationToken);
                if (conditionalResponse.StatusCode == HttpStatusCode.NotModified)
                    return new Response(cachedResponse);

                var newResponse = new InternalResponse(conditionalResponse);
                await BlobCache.LocalMachine.InsertObject(key, newResponse, TimeSpan.FromMinutes(30));
                return conditionalResponse;
            }

            var resp = await _httpClient.Send(request, cancellationToken);
            cachedResponse = new InternalResponse(resp);
            await BlobCache.LocalMachine.InsertObject(key, cachedResponse, TimeSpan.FromMinutes(30));
            return resp;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private class Response : IResponse
        {
            public Response(InternalResponse internalResponse)
            {
                Headers = new ReadOnlyDictionary<string, string>(internalResponse.Headers);
                Body = internalResponse.Body;
                ApiInfo = internalResponse.ApiInfo;
                StatusCode = internalResponse.StatusCode;
                ContentType = internalResponse.ContentType;
            }

            /// <summary>
            /// Raw response body. Typically a string, but when requesting images, it will be a byte array.
            /// </summary>
            public object Body { get; private set; }
            /// <summary>
            /// Information about the API.
            /// </summary>
            public IReadOnlyDictionary<string, string> Headers { get; private set; }
            /// <summary>
            /// Information about the API response parsed from the response headers.
            /// </summary>
            public ApiInfo ApiInfo { get; private set; }
            /// <summary>
            /// The response status code.
            /// </summary>
            public HttpStatusCode StatusCode { get; private set; }
            /// <summary>
            /// The content type of the response.
            /// </summary>
            public string ContentType { get; private set; }
        }

        private class InternalResponse
        {
            public string Body { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public ApiInfo ApiInfo { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public string ContentType { get; set; }

            public InternalResponse()
            {
            }

            public InternalResponse(IResponse response)
            {
                Body = response.Body as string;
                Headers = new Dictionary<string, string>();
                foreach (var h in response.Headers)
                    Headers.Add(h.Key, h.Value);

                //ApiInfo = response.ApiInfo;
                StatusCode = response.StatusCode;
                ContentType = response.ContentType;
            }
        }
    }
}

