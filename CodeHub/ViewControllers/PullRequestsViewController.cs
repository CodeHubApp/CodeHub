using System;
using CodeFramework.Controllers;
using CodeHub.Controllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace CodeHub.ViewControllers
{
    public class PullRequestsViewController : BaseListControllerDrivenViewController, IListView<PullRequestModel>
    {
        private UISegmentedControl _viewSegment;
        private UIBarButtonItem _segmentBarButton;

        public new PullRequestsController Controller
        {
            get { return (PullRequestsController)base.Controller; }
            set { base.Controller = value; }
        }

        public PullRequestsViewController(string user, string repo)
        {
            Root.UnevenRows = true;
            Title = "Pull Requests".t();
            SearchPlaceholder = "Search Pull Requests".t();
            NoItemsText = "No Pull Requests".t();
            Controller = new PullRequestsController(this, user, repo);
        }

        public void Render(ListModel<PullRequestModel> model)
        {
            RenderList(model, s => {
                var sse = new NameTimeStringElement {
                    Name = s.Title,
                    String = s.Body.Replace('\n', ' ').Replace("\r", ""),
                    Lines = 3,
                    Time = s.CreatedAt.ToDaysAgo(),
                    Image = CodeFramework.Images.Misc.Anonymous,
                    ImageUri = new Uri(s.User.AvatarUrl)
                };
                sse.Tapped += () => NavigationController.PushViewController(new PullRequestViewController(Controller.User, Controller.Repo, s.Number), true);
                return sse;
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _viewSegment = new UISegmentedControl(new string[] { "Open".t(), "Closed".t() });
            _viewSegment.ControlStyle = UISegmentedControlStyle.Bar;
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
        }

        
        protected override void SearchEnd()
        {
            base.SearchEnd();
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null && !IsSearching)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;

            //Select which one is currently selected
            _viewSegment.SelectedSegment = Controller.Filter.IsOpen ? 0 : 1;

            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            if (_viewSegment.SelectedSegment == 0)
            {
                Controller.ApplyFilter(new CodeHub.Filters.Models.PullRequestsFilterModel { IsOpen = true }, true, false);
                UpdateAndRender();
            }
            else if (_viewSegment.SelectedSegment == 1)
            {
                Controller.ApplyFilter(new CodeHub.Filters.Models.PullRequestsFilterModel { IsOpen = false }, true, false);
                UpdateAndRender();
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

