using System;
using System.Net.Http;

namespace CodeFramework.Core.Services
{
    public interface IHttpClientService
    {
		HttpClient Create();
    }
}

