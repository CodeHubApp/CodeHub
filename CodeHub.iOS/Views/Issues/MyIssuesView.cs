using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System;

namespace CodeHub.iOS.Views.Issues
{
    public class MyIssuesView : BaseIssuesView
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed", "Custom" });
        private UIBarButtonItem _segmentBarButton;

        public static MyIssuesView Create()
            => new MyIssuesView { ViewModel = new MyIssuesViewModel() };

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            var vm = (MyIssuesViewModel)ViewModel;
            var weakVm = new WeakReference<MyIssuesViewModel>(vm);

            vm.Bind(x => x.SelectedFilter).Subscribe(x =>
            {
                var goodVm = weakVm.Get();

                if (x == 2 && goodVm != null)
                {
                    var filter = new ViewControllers.Filters.MyIssuesFilterViewController(goodVm.Issues);
                    var nav = new UINavigationController(filter);
                    PresentViewController(nav, true, null);
                }

                // If there is searching going on. Finish it.
                FinishSearch();
            });

            this.BindCollection(vm.Issues, CreateElement);

            OnActivation(d =>
            {
                d(vm.Bind(x => x.SelectedFilter, true).Subscribe(x => _viewSegment.SelectedSegment = (nint)x));
                d(_viewSegment.GetChangedObservable().Subscribe(x => vm.SelectedFilter = x));
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

