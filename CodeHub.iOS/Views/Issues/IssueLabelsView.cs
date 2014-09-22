using System;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : ReactiveTableViewController<IssueLabelsViewModel>
    {
        public IssueLabelsView()
        {
            Title = "Labels";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.BackButton, UIBarButtonItemStyle.Plain,
			    (s, e) =>
			    {
			        if (ViewModel.SaveOnSelect)
                        ViewModel.SelectLabelsCommand.ExecuteIfCan();
			    });

            TableView.Source = new IssueLabelTableViewSource(TableView, ViewModel.Labels);
        }
    }
}

