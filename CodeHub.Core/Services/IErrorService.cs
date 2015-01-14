using System;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public interface IErrorService
    {
        IObservable<IError> AddExtraInformation { get; }

        void Init(string sentryUrl, string sentryClientId, string sentrySecret);

        void ReportError(Exception e);
    }

    public interface IError
    {
        Exception Exception { get; }

        IDictionary<string, string> Extras { get; }
    }
}
