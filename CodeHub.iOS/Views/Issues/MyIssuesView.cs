using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System;

namespace CodeHub.iOS.Views.Issues
{
    public class MyIssuesView : BaseIssuesView
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });

        public static MyIssuesView Create()
            => new MyIssuesView { ViewModel = new MyIssuesViewModel() };

        public MyIssuesView()
        {
            NavigationItem.TitleView = _viewSegment;
        }

        private void ShowFilter()
        {
            var vm = (MyIssuesViewModel)ViewModel;
            var filter = new ViewControllers.Filters.MyIssuesFilterViewController(vm.Issues);
            var nav = new UINavigationController(filter);
            PresentViewController(nav, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (MyIssuesViewModel)ViewModel;
            var weakVm = new WeakReference<MyIssuesViewModel>(vm);

            vm.Bind(x => x.SelectedFilter).Subscribe(x =>
            {
                FinishSearch();
            });

            this.BindCollection(vm.Issues, CreateElement);

            var filterButton = new UIBarButtonItem(UIBarButtonSystemItem.Bookmarks);
            NavigationItem.RightBarButtonItem = filterButton;

            OnActivation(d =>
            {
                d(vm.Bind(x => x.SelectedFilter, true)
                  .Subscribe(x => _viewSegment.SelectedSegment = (nint)x));

                d(_viewSegment.GetChangedObservable()
                  .Subscribe(x => vm.SelectedFilter = x));

                d(filterButton.GetClickedObservable()
                  .Subscribe(_ => ShowFilter()));
            });
        }
    }
}

