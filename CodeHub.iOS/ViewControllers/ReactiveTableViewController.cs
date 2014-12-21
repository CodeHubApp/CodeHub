using System;
using MonoTouch.UIKit;
using System.Reactive.Linq;
using System.Drawing;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS;
using System.Threading.Tasks;

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
            this.WhenAnyValue(x => x.ViewModel)
                .OfType<IProvidesTitle>()
                .Select(x => x.WhenAnyValue(y => y.Title))
                .Switch().Subscribe(x => Title = x ?? string.Empty);
        }

        public override void LoadView()
        {
            base.LoadView();

            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };

            this.WhenActivated(d =>
            {
                // Always keep this around since it calls the VM WhenActivated
            });

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
                activityView.StartAnimating();
                TableView.TableFooterView = activityView;

                iLoadableViewModel.LoadCommand.IsExecuting.Where(x => !x).Skip(1).Take(1).Subscribe(_ =>
                {
                    TableView.TableFooterView = null;
                    TableView.ReloadData();
                    activityView.StopAnimating();

                    CreateRefreshControl();

                    var iSourceInformsEnd = TableView.Source as IInformsEnd;

                    if (iPaginatableViewModel != null && iSourceInformsEnd != null)
                    {
                        iSourceInformsEnd.RequestMore.Select(__ => iPaginatableViewModel.LoadMoreCommand).IsNotNull().Subscribe(async x =>
                        {
                            activityView.StartAnimating();
                            TableView.TableFooterView = activityView;

                            await x.ExecuteAsync();

                            TableView.TableFooterView = null;
                            activityView.StopAnimating();
                        });
                    }
                });
                iLoadableViewModel.LoadCommand.ExecuteIfCan();
            }
        }

        protected virtual void CreateRefreshControl()
        {
            var iLoadableViewModel = ViewModel as ILoadableViewModel;
            if (iLoadableViewModel != null)
                RefreshControl = ((ILoadableViewModel)ViewModel).LoadCommand.ToRefreshControl();
        }

        protected virtual void CreateSearchBar()
        {
            var searchableViewModel = ViewModel as IProvidesSearchKeyword;
            if (searchableViewModel != null)
                this.AddSearchBar(x => searchableViewModel.SearchKeyword = x);
        }
    }
}

