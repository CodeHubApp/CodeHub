using Cirrious.MvvmCross.Binding.BindingContext;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.User;
using MonoTouch.Dialog;
using MonoTouch.Dialog.Utilities;

namespace CodeHub.iOS.Views.User
{
    public class ProfileView : ViewModelDrivenViewController, IImageUpdated
    {
        private HeaderView _header;

        public new ProfileViewModel ViewModel
        {
            get { return (ProfileViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            Title = "Profile";

            base.ViewDidLoad();

            _header = new HeaderView(View.Bounds.Width);

            var set = this.CreateBindingSet<ProfileView, ProfileViewModel>();
            set.Bind(_header).For(x => x.Title).To(x => x.Username).OneWay();
            set.Apply();

            var followers = new StyledStringElement("Followers".t(), () => ViewModel.GoToFollowersCommand.Execute(null), Images.Heart);
            var following = new StyledStringElement("Following".t(), () => ViewModel.GoToFollowingCommand.Execute(null), Images.Following);
            var events = new StyledStringElement("Events".t(), () => ViewModel.GoToEventsCommand.Execute(null), Images.Event);
            var organizations = new StyledStringElement("Organizations".t(), () => ViewModel.GoToOrganizationsCommand.Execute(null), Images.Group);
            var repos = new StyledStringElement("Repositories".t(), () => ViewModel.GoToRepositoriesCommand.Execute(null), Images.Repo);
            var gists = new StyledStringElement("Gists", () => ViewModel.GoToGistsCommand.Execute(null), Images.Script);

            Root.Add(new Section(_header));
            Root.Add(new [] { new Section { events, organizations, followers, following }, new Section { repos, gists } });
        }

        public void UpdatedImage (System.Uri uri)
        {
            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
            if (_header.Image != null)
                _header.SetNeedsDisplay();
        }
    }
}

