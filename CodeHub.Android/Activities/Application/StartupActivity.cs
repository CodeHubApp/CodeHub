using Android.App;
using Android.OS;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using ReactiveUI;

namespace CodeHub.Android.Activities.Application
{
    [Activity(Label = "StartupActivity", MainLauncher = true, Icon = "@drawable/icon")]
    public class StartupActivity : ReactiveActivity<StartupViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
//            ViewModel = new StartupViewModel(IoC.Resolve<IAccountsService>(), IoC.Resolve<ILoginService>());
//
//            this.WhenActivated(d =>
//            {
//                ViewModel.LoadCommand.ExecuteIfCan();
//            });
        }
    }
}