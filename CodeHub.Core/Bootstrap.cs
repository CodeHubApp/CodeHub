using Xamarin.Utilities.Core.Services;
using ReactiveUI;
using System.Reactive;
using System;
using Splat;

namespace CodeHub.Core
{
    /// <summary>
    /// Define the App type.
    /// </summary>
    public static class Bootstrap
    {
        public static void Init()
        {
            RxApp.DefaultExceptionHandler = Observer.Create((Exception e) => 
                IoC.Resolve<IAlertDialogService>().Alert("Error", e.Message));

            //Locator.CurrentMutable.RegisterConstant(new ConsoleLogger(), typeof(ILogger));

            var httpService = IoC.Resolve<IHttpClientService>();
			GitHubSharp.Client.ClientConstructor = httpService.Create;
        }

        private class ConsoleLogger : ILogger
        {
            public void Write(string message, LogLevel logLevel)
            {
                Console.WriteLine("[{0}] - {1}", logLevel, message);
            }

            public LogLevel Level { get; set; }
        }
    }
}