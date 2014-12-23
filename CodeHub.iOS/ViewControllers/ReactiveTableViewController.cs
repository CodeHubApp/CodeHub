using System;
using MonoTouch.UIKit;
using System.Reactive.Linq;
using System.Drawing;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS;

namespace Xamarin.Utilities.ViewControllers
{
    public abstract class NewReactiveTableViewController<TViewModel> : ReactiveTableViewController, IViewFor<TViewModel> where TViewModel : class
    {
        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set { this.RaiseAndSetIfChanged(ref _viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return _viewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        protected NewReactiveTableViewController()
            : this(UITableViewStyle.Plain)
        {
        }

        protected NewReactiveTableViewController(UITableViewStyle style)
            : base(style)
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };

            this.WhenAnyValue(x => x.ViewModel)
                .OfType<IProvidesTitle>()
                .Select(x => x.WhenAnyValue(y => y.Title))
                .Switch().Subscribe(x => Title = x ?? string.Empty);

            this.WhenActivated(d =>
            {
                // Always keep this around since it calls the VM WhenActivated
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateSearchBar();
            LoadViewModel();
        }

        protected virtual void LoadViewModel()
        {
            var iLoadableViewModel = ViewModel as ILoadableViewModel;
            var iPaginatableViewModel = ViewModel as IPaginatableViewModel;

            if (iLoadableViewModel != null)
            {
                var activityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White) { 
                    Frame = new RectangleF(0, 0, 320f, 88f),
                    Color = Theme.PrimaryNavigationBarColor,
                };

                var refreshControl = new UIRefreshControl();
                refreshControl.ValueChanged += async (sender, e) => {
                    if (iLoadableViewModel.LoadCommand.CanExecute(null))
                    {
                        await iLoadableViewModel.LoadCommand.ExecuteAsync();
                        refreshControl.EndRefreshing();
                    }
                };

                iLoadableViewModel.LoadCommand.IsExecuting
                    .Where(x => x && !refreshControl.Refreshing).Subscribe(_ =>
                    {
                        var rows = 0;
                        if (TableView.Source != null)
                        {
                            for (var i = 0; i < TableView.Source.NumberOfSections(TableView); i++)
                                rows += TableView.Source.RowsInSection(TableView, i);
                        }

                        if (rows == 0)
                        {
                            activityView.StartAnimating();
                            TableView.TableFooterView = activityView;
                            RefreshControl = null;
                        }
                    });

                iLoadableViewModel.LoadCommand.IsExecuting
                    .Where(x => !x).Subscribe(_ =>
                    {
                        activityView.StopAnimating();
                        if (TableView.TableFooterView != null)
                        {
                            TableView.TableFooterView = null;
                            TableView.ReloadData();
                        }

                        if (RefreshControl == null)
                        {
                            RefreshControl = refreshControl;
                        }
                    });

                this.WhenActivated(d =>
                {
                    var iSourceInformsEnd = TableView.Source as IInformsEnd;
                    if (iPaginatableViewModel != null && iSourceInformsEnd != null)
                    {
                        d(iSourceInformsEnd.RequestMore.Select(__ => iPaginatableViewModel.LoadMoreCommand).IsNotNull().Subscribe(async x =>
                        {
                            activityView.StartAnimating();
                            TableView.TableFooterView = activityView;

                            await x.ExecuteAsync();

                            TableView.TableFooterView = null;
                            activityView.StopAnimating();
                        }));
                    }
                });

                iLoadableViewModel.LoadCommand.ExecuteIfCan();
            }
        }

        protected virtual void CreateSearchBar()
        {
            var searchableViewModel = ViewModel as IProvidesSearchKeyword;
            if (searchableViewModel != null)
                this.AddSearchBar(x => searchableViewModel.SearchKeyword = x);
        }
    }
}

