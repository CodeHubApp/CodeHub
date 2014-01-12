using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : ViewModelCollectionDrivenDialogViewController
    {
		private readonly UISegmentedControl _viewSegment;
		private readonly UIBarButtonItem _segmentBarButton;
 
		public PullRequestsView()
		{
			Root.UnevenRows = true;
			Title = "Pull Requests".t();
			NoItemsText = "No Pull Requests".t();

			_viewSegment = new UISegmentedControl(new object[] { "Open".t(), "Closed".t() });
			_segmentBarButton = new UIBarButtonItem(_viewSegment);
			ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			var vm = (PullRequestsViewModel)ViewModel;
            _segmentBarButton.Width = View.Frame.Width - 10f;
			var set = this.CreateBindingSet<PullRequestsView, PullRequestsViewModel>();
			set.Bind(_viewSegment).To(x => x.SelectedFilter);
			set.Apply();

			BindCollection(vm.PullRequests, s =>
            {
                var sse = new NameTimeStringElement
                {
                    Name = s.Title,
                    String = s.Body.Replace('\n', ' ').Replace("\r", ""),
                    Lines = 3,
                    Time = s.CreatedAt.ToDaysAgo(),
                    Image = Theme.CurrentTheme.AnonymousUserImage,
                    ImageUri = new Uri(s.User.AvatarUrl)
                };
					sse.Tapped += () => vm.GoToPullRequestCommand.Execute(s);
                return sse;
            });
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

