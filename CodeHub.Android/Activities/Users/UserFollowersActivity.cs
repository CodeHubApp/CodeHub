using Android.App;
using Android.OS;
using Android.Widget;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Android.Activities.Users
{
    [Activity(Label = "UserFollowersActivity", MainLauncher = true, Icon = "@drawable/icon")]
    public class UserFollowersActivity : ReactiveActivity<UserFollowersViewModel>
    {
        public ListView List { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.UsersLayout);
            this.WireUpControls();

            var adapter = new ReactiveListAdapter<BasicUserModel>(ViewModel.Users, (vm, parent) => null);
            List.Adapter = adapter;
        }
    }
}