using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
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

            IoC.RegisterAssemblyServicesAsSingletons(typeof(Xamarin.Utilities.Core.Services.IDefaultValueService).Assembly);
            //IoC.RegisterAssemblyServicesAsSingletons(typeof(Xamarin.Utilities.Services.DefaultValueService).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(typeof(CodeFramework.Core.Services.IAccountsService).Assembly);
            //IoC.RegisterAssemblyServicesAsSingletons(typeof(CodeFramework.iOS.Theme).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(typeof(Core.Services.IApplicationService).Assembly);
            IoC.RegisterAssemblyServicesAsSingletons(GetType().Assembly);

            var viewModelViewService = IoC.Resolve<IViewModelViewService>();
            //viewModelViewService.RegisterViewModels(typeof(Xamarin.Utilities.Services.DefaultValueService).Assembly);
            //viewModelViewService.RegisterViewModels(typeof(CodeFramework.iOS.Theme).Assembly);
            viewModelViewService.RegisterViewModels(GetType().Assembly);

            //IoC.Resolve<IErrorService>().Init("http://sentry.dillonbuchanan.com/api/5/store/", "17e8a650e8cc44678d1bf40c9d86529b ", "9498e93bcdd046d8bb85d4755ca9d330");
            Core.Bootstrap.Init();
        }
    }
}