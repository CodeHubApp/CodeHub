using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using CodeHub.iOS.DialogElements;
using MvvmCross.Binding.BindingContext;
using System;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : ViewModelCollectionDrivenDialogViewController
    {
        private readonly UISegmentedControl _viewSegment;
 
        public PullRequestsView()
        {
            Title = "Pull Requests";
            NoItemsText = "No Pull Requests";

            _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            NavigationItem.TitleView = _viewSegment;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;

            var vm = (PullRequestsViewModel)ViewModel;
            var set = this.CreateBindingSet<PullRequestsView, PullRequestsViewModel>();
            set.Bind(_viewSegment).To(x => x.SelectedFilter);
            set.Apply();

            var weakVm = new WeakReference<PullRequestsViewModel>(vm);
            BindCollection(vm.PullRequests, s => new PullRequestElement(s, () => weakVm.Get()?.GoToPullRequestCommand.Execute(s)));
        }
    }
}

