using Xamarin.Utilities.Core.Services;
using ReactiveUI;
using System.Reactive;
using System;
using CodeFramework.Core.ViewModels.Application;

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
            {
                IoC.Resolve<IAlertDialogService>().Alert("Error", e.Message);
            });

            var httpService = IoC.Resolve<IHttpClientService>();
			GitHubSharp.Client.ClientConstructor = httpService.Create;

            IoC.RegisterAsInstance<IAddAccountViewModel, CodeHub.Core.ViewModels.Accounts.NewAccountViewModel>();
            IoC.RegisterAsInstance<IMainViewModel, CodeHub.Core.ViewModels.App.MenuViewModel>();
        }
    }
}