using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.Android
{
    [Application(Label = "CodeHub")]
    public class Application : global::Android.App.Application
    {
        Application(IntPtr handle, JniHandleOwnership owner)
            : base(handle, owner)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            IoC.Register(this.ApplicationContext);

            IoC.RegisterAssemblyServicesAsSingletons(typeof(Xamarin.Utilities.Core.Services.IDefaultValueService).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(typeof(Xamarin.Utilities.Android.Services.HttpClientService).Assembly);
            //IoC.RegisterAssemblyServicesAsSingletons(typeof(Xamarin.Utilities.Services.DefaultValueService).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(typeof(Core.Services.IAccountsService).Assembly);
            //IoC.RegisterAssemblyServicesAsSingletons(typeof(CodeFramework.iOS.Theme).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(GetType().Assembly);

            var viewModelViewService = IoC.Resolve<IViewModelViewService>();
            //viewModelViewService.RegisterViewModels(typeof(Xamarin.Utilities.Services.DefaultValueService).Assembly);
            viewModelViewService.RegisterViewModels(typeof(Xamarin.Utilities.Android.Services.HttpClientService).Assembly);
            viewModelViewService.RegisterViewModels(GetType().Assembly);

            IoC.Resolve<IAccountsService>().Insert(new GitHubAccount
            {
                Username = "thedillonb",
                OAuth = "5123a09eed74ded9b555eb02772c986a72f4744a",
                Domain = "https://api.github.com",
                IsEnterprise = false,
            });

            //IoC.Resolve<IErrorService>().Init("http://sentry.dillonbuchanan.com/api/5/store/", "17e8a650e8cc44678d1bf40c9d86529b ", "9498e93bcdd046d8bb85d4755ca9d330");
            Core.Bootstrap.Init();
        }
    }
}