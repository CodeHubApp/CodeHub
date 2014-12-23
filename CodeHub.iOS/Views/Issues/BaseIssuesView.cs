using CodeHub.Core.ViewModels.Issues;
using Xamarin.Utilities.ViewControllers;
using CodeHub.iOS.TableViewSources;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Issues
{
    public abstract class BaseIssuesView<TViewModel> : NewReactiveTableViewController<TViewModel> where TViewModel : class, IBaseIssuesViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new IssueTableViewSource(TableView);
            TableView.Source = source;
            this.WhenAnyValue(x => x.ViewModel.GroupedIssues).IsNotNull().Subscribe(source.SetData);
        }
    }
}

