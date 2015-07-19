using System;
using Octokit.Internal;
using Octokit;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using ModernHttpClient;

namespace CodeHub.Core.Utilities
{
    public class OctokitModernHttpClient : IHttpClient
    {
        private readonly MessageBuilder _messageBuilder = new MessageBuilder(null);

        public static Func<NativeMessageHandler> CreateMessageHandler = () => new NativeMessageHandler();

        /// <summary>
        /// Sends the specified request and returns a response.
        /// </summary>
        /// <param name="request">A <see cref="IRequest"/> that represents the HTTP request</param>
        /// <param name="cancellationToken">Used to cancel the request</param>
        /// <returns>A <see cref="Task" /> of <see cref="IResponse"/></returns>
        public async Task<IResponse> Send(IRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var httpOptions = CreateMessageHandler();
            httpOptions.AllowAutoRedirect = request.AllowAutoRedirect;

            if (httpOptions.SupportsAutomaticDecompression)
            {
                httpOptions.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var http = new HttpClient(httpOptions)
            {
                BaseAddress = request.BaseAddress,
                Timeout = request.Timeout
            };
            using (var requestMessage = _messageBuilder.BuildRequestMessage(request))
            {
                // Make the request
                var responseMessage = await http.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken)
                    .ConfigureAwait(false);
                return await _messageBuilder.BuildResponse(responseMessage).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        /// This class' only purpose is so I can call the helper methods.
        /// </summary>
        private class MessageBuilder : HttpClientAdapter
        {
            public MessageBuilder(Func<HttpMessageHandler> getHandler)
                : base(getHandler)
            {
            }

            public new HttpRequestMessage BuildRequestMessage(IRequest request)
            {
                return base.BuildRequestMessage(request);
            }

            public new Task<IResponse> BuildResponse(HttpResponseMessage responseMessage)
            {
                return base.BuildResponse(responseMessage);
            }
        }
    }
}

