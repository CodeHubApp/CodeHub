using System;
using ReactiveUI;
using MonoTouch.UIKit;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels;
using System.Drawing;
using System.Reactive.Subjects;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views
{
    public abstract class BaseTableViewController<TViewModel> : BaseTableViewController, IViewFor<TViewModel> where TViewModel : class
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

        protected BaseTableViewController()
            : this(UITableViewStyle.Plain)
        {
        }

        protected BaseTableViewController(UITableViewStyle style)
            : base(style)
        {
            this.WhenAnyValue(x => x.ViewModel)
                .OfType<ILoadableViewModel>()
                .Subscribe(x => x.LoadCommand.ExecuteIfCan());

            this.WhenAnyValue(x => x.ViewModel)
                .OfType<IProvidesTitle>()
                .Select(x => x.WhenAnyValue(y => y.Title))
                .Switch().Subscribe(x => Title = x ?? string.Empty);

            this.WhenActivated(d => { });
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
            }
        }

        protected virtual void CreateSearchBar()
        {
            var searchableViewModel = ViewModel as IProvidesSearchKeyword;
            if (searchableViewModel != null)
                this.AddSearchBar(x => searchableViewModel.SearchKeyword = x);
        }
    }

    public abstract class BaseTableViewController : ReactiveTableViewController
    {
        private readonly ISubject<bool> _appearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _appearedSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearedSubject = new Subject<bool>();

        public IObservable<bool> Appearing
        {
            get { return _appearingSubject; }
        }

        public IObservable<bool> Appeared
        {
            get { return _appearedSubject; }
        }

        public IObservable<bool> Disappearing
        {
            get { return _disappearingSubject; }
        }

        public IObservable<bool> Disappeared
        {
            get { return _disappearedSubject; }
        }

        protected BaseTableViewController(UITableViewStyle style)
            : base(style)
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _appearingSubject.OnNext(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _appearedSubject.OnNext(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            _disappearingSubject.OnNext(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _disappearedSubject.OnNext(animated);
        }
    }
}

