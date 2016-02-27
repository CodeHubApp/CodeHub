using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using UIKit;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Source
{
    public class BranchesAndTagsView : ViewModelCollectionDrivenDialogViewController
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});

        public BranchesAndTagsView()
        {
            Title = "Source";
            NavigationItem.TitleView = _viewSegment;

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), "There are no items."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (BranchesAndTagsViewModel)ViewModel;
            var weakVm = new WeakReference<BranchesAndTagsViewModel>(vm);

            BindCollection(vm.Items, x => {
                var e = new StringElement(x.Name);
                e.Clicked.Subscribe(MakeCallback(weakVm, x));
                return e;
            });

            OnActivation(d =>
            {
                d(vm.Bind(x => x.SelectedFilter, true).Subscribe(x => _viewSegment.SelectedSegment = (nint)x));
                d(_viewSegment.GetChangedObservable().Subscribe(_ => vm.SelectedFilter = (int)_viewSegment.SelectedSegment));
            });
        }

        private static Action<object> MakeCallback(WeakReference<BranchesAndTagsViewModel> weakVm, BranchesAndTagsViewModel.ViewObject viewObject)
        {
            return new Action<object>(_ => weakVm.Get()?.GoToSourceCommand.Execute(viewObject));
        }
    }
}

