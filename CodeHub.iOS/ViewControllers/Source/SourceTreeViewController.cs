using System;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using UIKit;
using System.Reactive.Linq;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class SourceTreeViewController : BaseTableViewController<SourceTreeViewModel>
    {
        public SourceTreeViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileDirectory.ToEmptyListImage(), "This directory is empty."));

            OnActivation(d => 
                d(this.WhenAnyValue(x => x.ViewModel.CanAddFile, x => x.ViewModel.GoToAddFileCommand)
                .Where(x => x.Item1)
                .Select(x => x.Item2)
                .ToBarButtonItem(UIBarButtonSystemItem.Add, x => NavigationItem.RightBarButtonItem = x)));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new SourceContentTableViewSource(TableView, ViewModel.Content);
        }
    }
}

