using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CodeFramework.Core.Services
{
    public class ErrorService : IErrorService
    {
        private readonly static string CrashReportFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "crash_report.json");
        private readonly IJsonSerializationService _jsonSerialization;
        private readonly HttpClient _httpClient;
        private readonly IEnvironmentService _environmentService;
        private readonly IAccountsService _accountsService;
        private string _sentryUrl;
        private string _sentryClientId;
        private string _sentrySecret;

        private static bool CrashReportExists
        {
            get { return System.IO.File.Exists(CrashReportFile); }
        }

        public ErrorService(IHttpClientService httpClient, IJsonSerializationService jsonSerialization, IEnvironmentService environmentService, IAccountsService accountsService)
        {
            _jsonSerialization = jsonSerialization;
            _environmentService = environmentService;
            _accountsService = accountsService;
            _httpClient = httpClient.Create();
            _httpClient.Timeout = new TimeSpan(0, 0, 10);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Init(string sentryUrl, string sentryClientId, string sentrySecret)
        {
            _sentryUrl = sentryUrl;
            _sentryClientId = sentryClientId;
            _sentrySecret = sentrySecret;

            if (CrashReportExists)
                SendPersistedRequest();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => LogException(e.ExceptionObject as Exception, true);
        }

		public void ReportError(Exception e)
		{
            LogException(e, false);

			try
			{
				Flurry.Analytics.Portable.AnalyticsApi.LogError (e);
			}
			catch {
			}
		}

        private void LogException(Exception exception, bool fatal)
        {
            Debug.WriteLine(exception.Message + " - " + exception.StackTrace);

            if (Debugger.IsAttached)
                Debugger.Break();
            else
            {
                try
                {
                    var request = new SentryRequest(exception);

                    // Add tags for easier sorting
                    request.Tags.Add("version", _environmentService.OSVersion);
                    request.Tags.Add("bundle_version", _environmentService.ApplicationVersion);
                    request.Tags.Add("fatal", fatal.ToString());

                    // Add some extras
                    request.Extra.Add("device_name", _environmentService.DeviceName);
                    request.Extra.Add("username", _accountsService.ActiveAccount != null ? _accountsService.ActiveAccount.Username : "No User");

                    if (fatal)
                    {
                        PersistRequest(request);
                    }
                    else
                    {
                        // Send it out the door
                        SendRequest(request);
                    }
                }
                catch
                {
                    Debug.WriteLine("Unable to report exception: " + exception.Message);
                }
            }
        }

        private void PersistRequest(object request)
        {
            System.IO.File.WriteAllText(CrashReportFile, _jsonSerialization.Serialize(request), Encoding.UTF8);
        }

        private void SendPersistedRequest()
        {
            try
            {
                var fileData = System.IO.File.ReadAllText(CrashReportFile, Encoding.UTF8);
                System.IO.File.Delete(CrashReportFile);
                SendRequest(_jsonSerialization.Deserialize<SentryRequest>(fileData));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to deserialize sentry request after crash: " + e.Message);
            }
        }

        private void SendRequest(SentryRequest request)
        {
            var header = String.Format("Sentry sentry_version={0}"
                + ", sentry_client={1}"
                + ", sentry_timestamp={2}"
                + ", sentry_key={3}"
                + ", sentry_secret={4}",
                5,
                "CodeHub/1.0",
                (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds,
                _sentryClientId,
                _sentrySecret);


            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(_sentryUrl));
            req.Headers.Add("X-Sentry-Auth", header);
            var requestData = _jsonSerialization.Serialize(request);
            req.Content = new StringContent(requestData, Encoding.UTF8, "application/json");
            _httpClient.SendAsync(req).ContinueWith(t =>
            {
                if (t.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                    Debug.WriteLine("Unable to send sentry analytic");
            });
        }

        private class SentryRequest
        {
            public SentryRequest()
            {
            }

            public SentryRequest(Exception exception)
            {
                Platform = "csharp";
                EventId = Guid.NewGuid().ToString("N");
                Timestamp = DateTimeOffset.UtcNow.ToString("o");
                Tags = new Dictionary<string, string>();
                Extra = new Dictionary<string, string>();

                Message = exception.Message;

                if (exception.TargetSite != null)
                {
                    Culprit = String.Format("{0} in {1}", ((exception.TargetSite.ReflectedType == null)
                        ? "<dynamic type>" : exception.TargetSite.ReflectedType.FullName), exception.TargetSite.Name);
                }

                Exception = new List<SentryException>();
                for (Exception currentException = exception;
                    currentException != null;
                    currentException = currentException.InnerException)
                {
                    SentryException sentryException = new SentryException(currentException)
                    {
                        Module = currentException.Source,
                        Type = currentException.GetType().Name,
                        Value = currentException.Message
                    };

                    Exception.Add(sentryException);
                }
            }

            public string EventId { get; set; }
            public string Culprit { get; set; }
            public string Timestamp { get; set; }
            public string Message { get; set; }
            public Dictionary<string, string> Tags { get; set; }
            public List<SentryException> Exception { get; set; }
            public Dictionary<string, string> Extra { get; set; }
            public string Platform { get; set; }

            public class SentryException
            {
                public SentryException()
                {
                }

                public SentryException(Exception exception)
                {
                    if (exception == null)
                        return;

                    Module = exception.Source;
                    Type = exception.GetType().FullName;
                    Value = exception.Message;

                    Stacktrace = new SentryStacktrace(exception);
                    if (Stacktrace.Frames == null || Stacktrace.Frames.Length == 0)
                        Stacktrace = null;
                }

                public string Type { get; set; }
                public string Value { get; set; }
                public string Module { get; set; }
                public SentryStacktrace Stacktrace { get; set; }

                public class SentryStacktrace
                {
                    public SentryStacktrace()
                    {
                    }

                    public SentryStacktrace(Exception exception)
                    {
                        if (exception == null)
                            return;

                        var trace = new StackTrace(exception, true);
                        var frames = trace.GetFrames();

                        if (frames == null)
                            return;

                        int length = frames.Length;
                        Frames = new SentryStackFrames[length];

                        for (int i = 0; i < length; i++)
                        {
                            StackFrame frame = trace.GetFrame(i);
                            Frames[i] = new SentryStackFrames(frame);
                        }
                    }

                    public SentryStackFrames[] Frames { get; set; }

                    public class SentryStackFrames
                    {
                        public SentryStackFrames()
                        {
                        }

                        public SentryStackFrames(StackFrame frame)
                        {
                            if (frame == null)
                                return;

                            int lineNo = frame.GetFileLineNumber();

                            if (lineNo == 0)
                            {
                                //The pdb files aren't currently available
                                lineNo = frame.GetILOffset();
                            }

                            var method = frame.GetMethod();
                            Filename = frame.GetFileName();
                            Module = (method.DeclaringType != null) ? method.DeclaringType.FullName : null;
                            Function = method.Name;
                            ContextLine = method.ToString();
                            Lineno = lineNo;
                            Colno = frame.GetFileColumnNumber();
                        }


                        public string Filename { get; set; }
                        public string Function { get; set; }
                        public int Lineno { get; set; }
                        public string ContextLine { get; set; }
                        public int Colno { get; set; }
                        public string Module { get; set; }
                    }
                }
            }
        }

    }
}

