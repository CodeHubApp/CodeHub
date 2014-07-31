using System;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : ViewModelCollectionViewController<PullRequestsViewModel>
    {
        private UISegmentedControl _viewSegment;
        private UIBarButtonItem _segmentBarButtonItem;

        public PullRequestsView()
            : base(unevenRows: true)
        {
        }

        public override void ViewDidLoad()
        {
            Title = "Pull Requests";
            //NoItemsText = "No Pull Requests";

            base.ViewDidLoad();

            _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });
            _segmentBarButtonItem = new UIBarButtonItem(_viewSegment) {Width = View.Frame.Width - 10f};
            ToolbarItems = new[] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButtonItem, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

            _viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = _viewSegment.SelectedSegment;
            ViewModel.WhenAnyValue(x => x.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = x);


            this.BindList(ViewModel.PullRequests, s =>
            {
                var sse = new NameTimeStringElement
                {
                    Name = s.Title ?? "No Title",
                    String = (s.Body ?? string.Empty).Replace('\n', ' ').Replace("\r", ""),
                    Lines = 3,
                    Time = s.CreatedAt.ToDaysAgo(),
                    Image = Theme.CurrentTheme.AnonymousUserImage,
                    ImageUri = new Uri(s.User.AvatarUrl)
                };
                sse.Tapped += () => ViewModel.GoToPullRequestCommand.Execute(s);
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

