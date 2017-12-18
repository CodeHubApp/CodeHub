using System;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using System.Linq;
using CoreGraphics;
using CodeHub.iOS.Transitions;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using System.Reactive;
using CodeHub.iOS.TableViewCells;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class TrendingRepositoriesViewController : TableViewController
    {
        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();
        private readonly TrendingTitleButton _trendingTitleButton = new TrendingTitleButton { Frame = new CGRect(0, 0, 200f, 32f) };

        public RepositoriesTrendingViewModel ViewModel { get; } = new RepositoriesTrendingViewModel();

        public TrendingRepositoriesViewController()
            : base(UITableViewStyle.Plain)
        {
            NavigationItem.TitleView = _trendingTitleButton;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableViewSource = new RepositoryTableViewSource(TableView);
            TableView.Source = tableViewSource;

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(ViewModel.LoadCommand);

            this.WhenActivated(d =>
            {
                d(ViewModel.LoadCommand.IsExecuting
                  .Subscribe(Loading));

                d(_trendingTitleButton.GetClickedObservable()
                  .Subscribe(_ => ShowLanguages()));

                d(ViewModel.WhenAnyValue(x => x.SelectedLanguage)
                  .Subscribe(x => _trendingTitleButton.Text = x.Name));

                d(ViewModel.RepositoryItemSelected
                  .Select(x => new RepositoryViewController(x.Owner, x.Name, x.Repository))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(ViewModel.WhenAnyValue(x => x.Items).Subscribe(items =>
                {
                    var sections = items.Select(item =>
                    {
                        var tsi = new TableSectionInformation<RepositoryItemViewModel, RepositoryCellView>(
                            new ReactiveList<RepositoryItemViewModel>(item.Item2),
                            RepositoryCellView.Key,
                            (float)UITableView.AutomaticDimension);
                        tsi.Header = new TableSectionHeader(() => CreateHeaderView(item.Item1), 26f);
                        return tsi;
                    });

                    tableViewSource.Data = sections.ToList();
                }));
            });
        }

        private void ShowLanguages()
        {
            var vm = new WeakReference<RepositoriesTrendingViewModel>(ViewModel as RepositoriesTrendingViewModel);
            var view = new LanguagesViewController();
            view.SelectedLanguage = vm.Get()?.SelectedLanguage;
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            view.NavigationItem.LeftBarButtonItem.GetClickedObservable().Subscribe(_ => DismissViewController(true, null));
            view.Language.Subscribe(x => {
                var viewModel = vm.Get();
                if (viewModel != null)
                    viewModel.SelectedLanguage = x;
                DismissViewController(true, null);
            });
            var ctrlToPresent = new ThemedNavigationController(view);
            ctrlToPresent.TransitioningDelegate = new SlideDownTransition();
            PresentViewController(ctrlToPresent, true, null);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
            {
                NavigationController.NavigationBar.ShadowImage = new UIImage();
                _trendingTitleButton.TintColor = NavigationController.NavigationBar.TintColor;
            }
        }

        private static UILabel CreateHeaderView(string name)
        {
            return new UILabel(new CGRect(0, 0, 320f, 26f)) 
            {
                BackgroundColor = Theme.CurrentTheme.PrimaryColor,
                Text = name,
                Font = UIFont.BoldSystemFontOfSize(14f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center
            };
        }

        private void Loading(bool searching)
        {
            _loading.SetLoading(searching);

            if (searching)
            {
                TableView.TableFooterView = _loading;
                TableView.BackgroundView = null;
            }
            else
            {
                TableView.TableFooterView = null;
            }
        }
    }
}

