using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using CodeHub.iOS.Elements;
using MvvmCross.Binding.BindingContext;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : ViewModelCollectionDrivenDialogViewController
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;
 
        public PullRequestsView()
        {
            Root.UnevenRows = true;
            Title = "Pull Requests";
            NoItemsText = "No Pull Requests";

            _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;

            var vm = (PullRequestsViewModel)ViewModel;
            _segmentBarButton.Width = View.Frame.Width - 10f;
            var set = this.CreateBindingSet<PullRequestsView, PullRequestsViewModel>();
            set.Bind(_viewSegment).To(x => x.SelectedFilter);
            set.Apply();

            BindCollection(vm.PullRequests, s => new PullRequestElement(s, () => vm.GoToPullRequestCommand.Execute(s)));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

