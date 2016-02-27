using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using CodeHub.iOS.DialogElements;
using System;
using GitHubSharp.Models;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : ViewModelCollectionDrivenDialogViewController
    {
        private readonly UISegmentedControl _viewSegment;
 
        public PullRequestsView()
        {
            Title = "Pull Requests";

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitPullRequest.ToImage(64f), "There are no pull requests.")); 

            _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            NavigationItem.TitleView = _viewSegment;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;

            var vm = (PullRequestsViewModel)ViewModel;
            var weakVm = new WeakReference<PullRequestsViewModel>(vm);
            BindCollection(vm.PullRequests, s => new PullRequestElement(s, MakeCallback(weakVm, s)));

            OnActivation(d =>
            {
                d(vm.Bind(x => x.SelectedFilter, true).Subscribe(x => _viewSegment.SelectedSegment = (nint)x));
                d(_viewSegment.GetChangedObservable().Subscribe(x => vm.SelectedFilter = x));
            });
        }

        private static Action MakeCallback(WeakReference<PullRequestsViewModel> weakVm, PullRequestModel model)
        {
            return new Action(() => weakVm.Get()?.GoToPullRequestCommand.Execute(model));
        }
    }
}

