using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using CodeHub.Core.Data;
using CodeHub.Core.Services;

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

            //IoC.Resolve<IErrorService>().Init("http://sentry.dillonbuchanan.com/api/5/store/", "17e8a650e8cc44678d1bf40c9d86529b ", "9498e93bcdd046d8bb85d4755ca9d330");
            Core.Bootstrap.Init();
        }
    }
}