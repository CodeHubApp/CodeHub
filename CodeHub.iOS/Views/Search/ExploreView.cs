using System;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using UIKit;
using CoreGraphics;
using CodeHub.Core.ViewModels.Search;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Search
{
    public class ExploreView : BaseTableViewController<ExploreViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Repositories", "Users" });

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.TitleView = _viewSegment;

            this.WhenAnyValue(x => x.ViewModel.SearchCommand)
                .Select(x => x.ToBarButtonItem(Images.Search))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            UISearchBar searchBar;
            var searchDelegate = this.AddSearchBar(out searchBar);

            var valueChanged = Observable.FromEventPattern(x => _viewSegment.ValueChanged += x, x => _viewSegment.ValueChanged -= x);

            var activityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White) { 
                Frame = new CGRect(0, 0, 320f, 88f),
                Color = NavigationController.NavigationBar.BackgroundColor,
            };

            this.WhenAnyValue(x => x.ViewModel.SearchFilter).Subscribe(x => {
                if (TableView.Source != null)
                {
                    TableView.Source.Dispose();
                    TableView.Source = null;
                }

                if (x == ExploreViewModel.SearchType.Repositories)
                    TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Repositories);
                else if (x == ExploreViewModel.SearchType.Users)
                    TableView.Source = new UserTableViewSource(TableView, ViewModel.Users);
            });

            this.WhenActivated(d => {
                d(valueChanged.Subscribe(_ => ViewModel.SearchFilter = (ExploreViewModel.SearchType)(int)_viewSegment.SelectedSegment));
                d(this.WhenAnyValue(x => x.ViewModel.SearchFilter).Subscribe(x => {
                    _viewSegment.SelectedSegment = (int)x;
                    TableView.ScrollRectToVisible(new CGRect(0, 0, 1, 1), false);
                    searchBar.BecomeFirstResponder();
                }));

                d(searchDelegate.SearchTextChanging.Subscribe(x => ViewModel.SearchText = x));
                d(this.WhenAnyValue(x => x.ViewModel.SearchText).Subscribe(x => searchBar.Text = x));
                d(searchDelegate.SearchTextChanged.Subscribe(x => ViewModel.SearchCommand.ExecuteIfCan()));

                d(ViewModel.SearchCommand.IsExecuting.Subscribe(x => {
                    if (x)
                    {
                        activityView.StartAnimating();
                        TableView.TableFooterView = activityView;
                    }
                    else
                    {
                        activityView.StopAnimating();
                        TableView.TableFooterView = null;
                        TableView.ReloadData();
                    }
                }));
            });
        }
    }
}

