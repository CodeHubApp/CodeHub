using System;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using CoreGraphics;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CodeHub.Core.ViewModels.Gists;
using System.Reactive;
using Splat;
using CodeHub.Core.Services;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class GistsViewController : TableViewController
    {
        private readonly Lazy<UISearchBar> _repositorySearchBar
            = new Lazy<UIKit.UISearchBar>(() => new UISearchBar(new CGRect(0, 0, 320, 44)));

        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();

        private readonly Lazy<UIView> emptyView = new Lazy<UIView>((() =>
            new EmptyListView(Octicon.Gist.ToEmptyListImage(), "There are no gists.")));

        public GistsViewModel ViewModel { get; }

        public static GistsViewController CreatePublicGistsViewController()
        {
            var viewModel = GistsViewModel.CreatePublicGistsViewModel();
            return new GistsViewController(viewModel)
            {
                Title = "Public Gists",
                ShowSearchBar = false
            };
        }

        public static GistsViewController CreateStarredGistsViewController()
        {
            var viewModel = GistsViewModel.CreateStarredGistsViewModel();
            return new GistsViewController(viewModel) { Title = "Starred Gists" };
        }

        public static GistsViewController CreateUserGistsViewController(string username)
        {
            var applicationService = Locator.Current.GetService<IApplicationService>();
            var mine = applicationService.Account.Username.ToLower().Equals(username.ToLower());

            if (mine)
            {
                var viewModel = new CurrentUserGistsViewModel(username);
                var vc = new GistsViewController(viewModel) { Title = "My Gists" };

                var button = new UIBarButtonItem(UIBarButtonSystemItem.Add);
                vc.NavigationItem.RightBarButtonItem = button;
                vc.WhenActivated(d =>
                {
                    d(button.GetClickedObservable()
                      .Subscribe(_ => GistCreateViewController.Show(vc)));
                });

                return vc;
            }
            else
            {
                var viewModel = GistsViewModel.CreateUserGistsViewModel(username);
                var vc = new GistsViewController(viewModel) { Title = $"{username}'s Gists" };
                return vc;
            }
        }

        public bool ShowSearchBar { get; private set; } = true;

        public GistsViewController(GistsViewModel viewModel)
            : base(UITableViewStyle.Plain)
        {
            ViewModel = viewModel;
            Title = "Gists";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableViewSource = new GistTableViewSource(TableView, ViewModel.Items);
            TableView.Source = tableViewSource;

            Appearing
                .Take(1)
                .Select(_ => ViewModel.LoadCommand.Execute())
                .Switch()
                .Take(1)
                .Catch(Observable.Return(false))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SetItemsPresent);

            this.WhenActivated(d =>
            {
                if (ShowSearchBar) {
                    d(_repositorySearchBar.Value.GetChangedObservable()
                        .Subscribe(x => ViewModel.SearchText = x));
                }

                d(ViewModel.ItemSelected
                  .Select(x => GistViewController.FromGist(x.Gist))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(ViewModel.WhenAnyValue(x => x.HasMore)
                  .Subscribe(x => TableView.TableFooterView = x ? _loading : null));

                d(tableViewSource.RequestMore
                  .InvokeReactiveCommand(ViewModel.LoadMoreCommand));

                d(ViewModel.LoadCommand
                  .Select(_ => ViewModel.Items.Changed)
                  .Switch()
                  .Select(_ => Unit.Default)
                  .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                  .Where(_ => TableView.LastItemVisible())
                  .InvokeReactiveCommand(ViewModel.LoadMoreCommand));

                d(ViewModel.LoadCommand.Merge(ViewModel.LoadMoreCommand)
                  .Select(_ => Unit.Default)
                  .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                  .Where(_ => TableView.LastItemVisible())
                  .InvokeReactiveCommand(ViewModel.LoadMoreCommand));
            });
        }

        private void SetItemsPresent(bool hasItems)
        {
            if (ShowSearchBar)
                TableView.TableHeaderView = hasItems ? _repositorySearchBar.Value : null;

            TableView.SeparatorStyle = hasItems
                ? UITableViewCellSeparatorStyle.SingleLine
                : UITableViewCellSeparatorStyle.None;

            if (hasItems)
            {
                TableView.BackgroundView = null;
            }
            else
            {
                emptyView.Value.Alpha = 0;
                TableView.BackgroundView = emptyView.Value;
                UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
                               () => emptyView.Value.Alpha = 1, null);
            }
        }
    }
}
