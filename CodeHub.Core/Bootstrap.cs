using ReactiveUI;
using System.Reactive;
using System;
using Splat;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;
using CodeHub.Core.Data;

namespace CodeHub.Core
{
    public static class Bootstrap
    {
        public static void Init()
        {
            RxApp.DefaultExceptionHandler = Observer.Create((Exception e) => 
                Locator.Current.GetService<IAlertDialogFactory>().Alert("Error", e.Message));
            
            var defaultValueService = Locator.Current.GetService<IDefaultValueService>();
            var accountService = new AccountsRepository(defaultValueService);
            var loginService = new LoginService(accountService);
            var analyticsService = new AnalyticsService();

            Locator.CurrentMutable.RegisterLazySingleton(() => accountService, typeof(IAccountsRepository));
            Locator.CurrentMutable.RegisterLazySingleton(() => analyticsService, typeof(IAnalyticsService));
            Locator.CurrentMutable.RegisterLazySingleton(() => new SessionService(accountService, analyticsService), typeof(ISessionService));
            Locator.CurrentMutable.RegisterLazySingleton(() => loginService, typeof(ILoginService));
            Locator.CurrentMutable.RegisterLazySingleton(() => new ImgurService(), typeof(IImgurService));
        }
    }
}