using Android.App;
using Android.OS;
using CodeHub.Core.ViewModels.Accounts;
using ReactiveUI;

namespace CodeHub.Android.Activities.Application
{
    [Activity(Label = "NewAcountActivity")]
    public class NewAcountActivity : ReactiveActivity<NewAccountViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            System.Console.WriteLine("Oh shit!");

            // Create your application here
        }
    }
}