using System;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using MonoTouch.UIKit;
using System.Drawing;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesExploreView : BaseTableViewController<RepositoriesExploreViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Repositories);
        
            var searchDelegate = this.AddSearchBar();

            var activityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White) { 
                Frame = new RectangleF(0, 0, 320f, 88f),
                Color = NavigationController.NavigationBar.BackgroundColor,
            };

            this.WhenActivated(d =>
            {
                d(searchDelegate.SearchTextChanged.Subscribe(x => 
                {
                    ViewModel.SearchText = x;
                    ViewModel.SearchCommand.ExecuteIfCan();
                }));

                d(ViewModel.SearchCommand.IsExecuting.Subscribe(x =>
                {
                    if (x)
                    {
                        activityView.StartAnimating();
                        TableView.TableFooterView = activityView;
                    }
                    else
                    {
                        activityView.StopAnimating();
                        TableView.TableFooterView = null;
                    }
                }));
            });
        }
    }
}

