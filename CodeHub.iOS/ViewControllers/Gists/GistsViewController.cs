using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;
using CoreGraphics;
using Octokit;
using ReactiveUI;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class GistsViewController : ListViewController<Gist>
    {
        private readonly Lazy<UISearchBar> _searchBar
            = new Lazy<UISearchBar>(() => new UISearchBar(new CGRect(0, 0, 320, 44)));

        public static GistsViewController CreatePublicGistsViewController()
            => FromGitHub(ApiUrls.PublicGists(), "Public Gists", false);

        public static GistsViewController CreateStarredGistsViewController()
            => FromGitHub(ApiUrls.StarredGists(), "Starred Gists");

        public static GistsViewController CreateUserGistsViewController(string username)
        {
            var applicationService = Locator.Current.GetService<IApplicationService>();
            var mine = applicationService.Account.Username.ToLower().Equals(username.ToLower());

            if (mine)
            {
                var vc = FromGitHub(ApiUrls.UsersGists(username), "My Gists");
                var button = new UIBarButtonItem(UIBarButtonSystemItem.Add);
                vc.NavigationItem.RightBarButtonItem = button;
                vc.WhenActivated(d =>
                {
                    d(button.GetClickedObservable()
                      .Subscribe(_ => GistCreateViewController.Show(vc)));
                });

                return vc;
            }

            return FromGitHub(ApiUrls.UsersGists(username), $"{username}'s Gists");
        }

        private static GistsViewController FromGitHub(
            Uri uri,
            string title,
            bool showSearchBar = true,
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            var dataRetriever = new GitHubList<Gist>(applicationService.GitHubClient, uri);
            return new GistsViewController(dataRetriever)
            {
                Title = title,
                ShowSearchBar = showSearchBar
            };
        }

        private static EmptyListView CreateEmptyListView()
            => new EmptyListView(Octicon.Gist.ToEmptyListImage(), "There are no gists!");

        private string _searchText;
        private string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public bool ShowSearchBar { get; private set; } = true;

        public GistsViewController(IDataRetriever<Gist> dataRetriever)
            : base(dataRetriever, CreateEmptyListView)
        {
            Title = "Gists";

            this.WhenActivated(d =>
            {
                if (ShowSearchBar)
                {
                    d(_searchBar.Value.GetChangedObservable()
                        .Subscribe(x => SearchText = x));
                }
            });
        }

        protected override Element ConvertToElement(Gist item)
        {
            var e = new GistElement(item);

            e.Clicked
             .Select(_ => new GistViewController(item))
             .Subscribe(this.PushViewController);
            
            return e;
        }
    }
}
