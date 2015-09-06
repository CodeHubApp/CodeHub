using System;

namespace CodeFramework.Core.Services
{
    public interface IErrorService
    {
        void Init(string sentryUrl, string sentryClientId, string sentrySecret);

        void ReportError(Exception e);
    }
}

