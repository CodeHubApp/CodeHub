using System;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using UIKit;
using CoreGraphics;
using CodeHub.Core.ViewModels.Search;
using System.Reactive.Linq;
using CoreText;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.ViewControllers.Search
{
    public class ExploreViewController : BaseViewController<ExploreViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Repositories", "Users" });
        private readonly UITableView _repositoryTableView = new UITableView();
        private readonly UITableView _userTableView = new UITableView();
        private readonly UISearchBar _repositorySearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
        private readonly UISearchBar _userSearchBar = new UISearchBar(new CGRect(0, 0, 320, 44));

        public ExploreViewController()
        {
            NavigationItem.TitleView = _viewSegment;

            _repositoryTableView.TableHeaderView = _repositorySearchBar;
            _userTableView.TableHeaderView = _userSearchBar;

            this.WhenAnyValue(x => x.ViewModel.Repositories.Items)
                .Subscribe(x => _repositoryTableView.Source = new RepositoryTableViewSource(_repositoryTableView, x)); 
            this.WhenAnyValue(x => x.ViewModel.Users.Items)
                .Subscribe(x => _userTableView.Source = new UserTableViewSource(_repositoryTableView, x)); 

            this.WhenActivated(d => {
                d(_viewSegment.GetChangedObservable().Subscribe(x => ViewModel.SearchFilter = (ExploreViewModel.SearchType)x));
//                d(searchDelegate.SearchTextChanging.Subscribe(x => ViewModel.SearchText = x));
//                d(this.WhenAnyValue(x => x.ViewModel.SearchText).Subscribe(x => searchBar.Text = x));
//                d(searchDelegate.SearchTextChanged.Subscribe(x => ViewModel.SearchCommand.ExecuteIfCan()));
                d(this.WhenAnyValue(x => x.ViewModel.SearchCommand)
                    .ToBarButtonItem(Images.Search, x => NavigationItem.RightBarButtonItem = x));

                //                d(this.WhenAnyValue(x => x.ViewModel.SearchFilter).Subscribe(x => {
                //                    _viewSegment.SelectedSegment = (int)x;
                //                    searchBar.BecomeFirstResponder();
                //                }));

                //                d(this.WhenAnyValue(x => x.ViewModel.SearchCommand).Select(x => x.IsExecuting).Switch().Subscribe(x => {
                //                    if (x)
                //                    {
                //                        activityView.StartAnimating();
                //                        TableView.TableFooterView = activityView;
                //                    }
                //                    else
                //                    {
                //                        activityView.StopAnimating();
                //                        TableView.TableFooterView = null;
                //                        TableView.ReloadData();
                //                    }
                //                }));

                d(this.WhenAnyValue(x => x.ViewModel.SearchFilter).Subscribe(x => {
                    _repositoryTableView.RemoveFromSuperview();
                    _userTableView.RemoveFromSuperview();
                    Add(x == ExploreViewModel.SearchType.Repositories ? _repositoryTableView : _userTableView);
                }));
            });
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            _repositoryTableView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _userTableView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _repositoryTableView.RemoveFromSuperview();
            _userTableView.RemoveFromSuperview();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

//            UISearchBar searchBar;
//            var searchDelegate = this.AddSearchBar(out searchBar);
//
            var activityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White) { 
                Frame = new CGRect(0, 0, 320f, 88f),
                Color = NavigationController.NavigationBar.BackgroundColor,
            };

//            this.WhenAnyValue(x => x.ViewModel.SearchFilter).Subscribe(x => {
//                _viewSegment.SelectedSegment = (int)x;
//                TableView.ScrollRectToVisible(new CGRect(0, 0, 1, 1), false);
//                searchBar.BecomeFirstResponder();
//            });


        }
    }


    public class TableView<TViewModel, TViewModelItem> : UITableView where TViewModel : IListViewModel<TViewModelItem>
    {
        private UISearchBar _searchBar;
        private TViewModel _viewModel;

        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;

                _searchBar?.RemoveFromSuperview();
                _searchBar?.Dispose();
                _searchBar = null;
                TableHeaderView = null;

                var canProvideSearch = value as CodeHub.Core.ViewModels.IProvidesSearchKeyword;
                if (canProvideSearch != null)
                {
                    _searchBar = new UISearchBar(new CGRect(0, 0, 320, 44));
                    TableHeaderView = _searchBar;
                    _searchBar.TextChanged += (sender, e) => canProvideSearch.SearchKeyword = _searchBar.Text;
                }
            }
        }
    }
}

