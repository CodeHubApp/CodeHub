using System;
using System.Reactive.Subjects;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public class ErrorService : IErrorService
    {
        private readonly Subject<IError> _errorSubject = new Subject<IError>();

        public IObservable<IError> AddExtraInformation
        {
            get { return _errorSubject; }
        }

        public void Init(string sentryUrl, string sentryClientId, string sentrySecret)
        {
        }

        public void ReportError(Exception e)
        {
            var error = new Error(e);
            _errorSubject.OnNext(error);

            // Do something with this...
        }

        private class Error : IError
        {
            public Exception Exception { get; private set; }
            public IDictionary<string, string> Extras { get; private set; }

            public Error(Exception exception)
            {
                Exception = exception;
                Extras = new Dictionary<string, string>();
            }
        }
    }
}

