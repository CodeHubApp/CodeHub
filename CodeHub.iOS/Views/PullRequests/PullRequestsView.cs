using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestsView : ViewModelCollectionDrivenViewController
    {
        private UISegmentedControl _viewSegment;
        private UIBarButtonItem _segmentBarButton;

        public new PullRequestsViewModel ViewModel
        {
            get { return (PullRequestsViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            Root.UnevenRows = true;
            Title = "Pull Requests".t();
            SearchPlaceholder = "Search Pull Requests".t();
            NoItemsText = "No Pull Requests".t();

            base.ViewDidLoad();

            _viewSegment = new UISegmentedControl(new object[] { "Open".t(), "Closed".t() });
            _viewSegment.ControlStyle = UISegmentedControlStyle.Bar;
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };


            BindCollection(ViewModel.PullRequests, s =>
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
                sse.Tapped += () => ViewModel.GoToPullRequestCommand.Execute(s);
                return sse;
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null && !IsSearching)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;

            //Select which one is currently selected
            _viewSegment.SelectedSegment = ViewModel.PullRequests.Filter.IsOpen ? 0 : 1;

            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            switch (_viewSegment.SelectedSegment)
            {
                case 0:
                    ViewModel.PullRequests.ApplyFilter(new Core.Filters.PullRequestsFilterModel { IsOpen = true }, true);
                    break;
                case 1:
                    ViewModel.PullRequests.ApplyFilter(new Core.Filters.PullRequestsFilterModel { IsOpen = false }, true);
                    break;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

