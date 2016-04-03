using System;
using CodeHub.Core.Services;
using System.Net.Http;
using CodeHub.Core;
using System.Text;
using UIKit;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CodeHub.iOS.Services
{
    public class ErrorService : IErrorService
    {
        private readonly string _appVersion, _systemVersion;

        private static string GetFilePath()
        {
            var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
            return Path.Combine (documents, "..", "tmp", "crash.log");
        }
        
        public ErrorService()
        {
            _appVersion = UIApplication.SharedApplication.GetVersion();
            _systemVersion = UIDevice.CurrentDevice.SystemVersion;
        }

        public void Init()
        {
            TaskScheduler.UnobservedTaskException += (sender, e) => {
                if (!e.Observed)
                {
                    SendError(e.Exception);
                    e.SetObserved();
                }
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                var ex = e.ExceptionObject as Exception;
                if (ex == null) return;

                if (e.IsTerminating)
                    WriteFile(ex);
                else
                    SendError(ex);
            };

            Task.Run(SendPersistedError);
        }

        public void Log(Exception e, bool fatal = false)
        {
            if (fatal)
            {
                WriteFile(e);
            }
            else
            {
                SendError(e);
            }
        }

        private void SendError(Exception e)
        {
            SendError(Serialize(e));
        }


        private static void SendError(string data)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://6o8w5n7wyc.execute-api.us-east-1.amazonaws.com/prod");
            request.Headers.Add("x-api-key", Secrets.ErrorReportingKey);
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            client.SendAsync(request).ToBackground();
        }

        private void WriteFile(Exception e)
        {
            var body = Serialize(e);
            File.WriteAllText(GetFilePath(), body, Encoding.UTF8);
        }

        private void SendPersistedError()
        {
            var path = GetFilePath();
            if (File.Exists(path))
            {
                var body = File.ReadAllText(path, Encoding.UTF8);
                SendError(body);
                File.Delete(path);
            }
        }

        private string Serialize(Exception e)
        {
            var sb = new StringBuilder();
            var ex = e;
            while (ex != null)
            {
                sb.AppendLine(ex.Message + ": " + ex.StackTrace);
                if (ex.InnerException != null)
                    sb.AppendLine("-------------");
                ex = ex.InnerException;
            }

            var error = new {
                e.Message,
                Stack = sb.ToString(),
                ApplicationVersion = _appVersion,
                SystemVersion = _systemVersion,
                TargetName = e.TargetSite?.Name
            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(error);
        }
    }
}

