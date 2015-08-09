using ReactiveUI;
using System.Reactive;
using System;
using Splat;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;
using CodeHub.Core.Data;
using System.Threading.Tasks;
using Octokit.Internal;

namespace CodeHub.Core
{
    public static class Bootstrap
    {
        public static void Init()
        {
            RxApp.DefaultExceptionHandler = Observer.Create((Exception e) => {
                if (e is TaskCanceledException)
                    e = new Exception("Timeout waiting for GitHub to respond!");
                Locator.Current.GetService<IAlertDialogFactory>().Alert("Error", e.Message);
            });
            
            var resolver = Locator.CurrentMutable;
            resolver.RegisterLazySingleton(() => new AccountsRepository(resolver.GetService<IDefaultValueService>()), typeof(IAccountsRepository));
            resolver.RegisterLazySingleton(() => new AnalyticsService(), typeof(IAnalyticsService));
            resolver.RegisterLazySingleton(() => new SessionService(resolver.GetService<IAccountsRepository>(), resolver.GetService<IAnalyticsService>()), typeof(ISessionService));
            resolver.RegisterLazySingleton(() => new LoginService(resolver.GetService<IAccountsRepository>()), typeof(ILoginService));
            resolver.RegisterLazySingleton(() => new ImgurService(), typeof(IImgurService));
            resolver.RegisterLazySingleton(() => new SimpleJsonSerializer(), typeof(IJsonSerializer));
            resolver.RegisterLazySingleton(() => new TrendingRepository(resolver.GetService<IJsonSerializer>()), typeof(ITrendingRepository));
        }

    }
}