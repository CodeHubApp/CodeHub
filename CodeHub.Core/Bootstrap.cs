using ReactiveUI;
using System.Reactive;
using System;
using Splat;
using Xamarin.Utilities.Services;
using CodeHub.Core.Services;
using Xamarin.Utilities.Factories;

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
                Locator.Current.GetService<IAlertDialogFactory>().Alert("Error", e.Message));

            var defaultValueService = Locator.Current.GetService<IDefaultValueService>();
            var accountService = new GitHubAccountsService(defaultValueService);
            var applicationService = new ApplicationService(accountService);
            var loginService = new LoginService(accountService);

            Locator.CurrentMutable.RegisterLazySingleton(() => accountService, typeof(IAccountsService));
            Locator.CurrentMutable.RegisterLazySingleton(() => applicationService, typeof(IApplicationService));
            Locator.CurrentMutable.RegisterLazySingleton(() => loginService, typeof(ILoginService));
        }
    }
}