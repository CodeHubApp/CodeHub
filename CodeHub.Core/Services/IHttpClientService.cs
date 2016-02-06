using System;
using System.Net.Http;

namespace CodeHub.Core.Services
{
    public interface IHttpClientService
    {
		HttpClient Create();
    }
}

