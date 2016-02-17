using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using UIKit;
using CodeHub.iOS.DialogElements;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Source
{
	public class BranchesAndTagsView : ViewModelCollectionDrivenDialogViewController
	{
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});

        public BranchesAndTagsView()
        {
            Title = "Source";
            NoItemsText = "No Items";
            NavigationItem.TitleView = _viewSegment;
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var vm = (BranchesAndTagsViewModel)ViewModel;
            var weakVm = new WeakReference<BranchesAndTagsViewModel>(vm);

            BindCollection(vm.Items, x => {
                var e = new StringElement(x.Name);
                e.Clicked.Select(_ => x).Subscribe(_ => weakVm.Get()?.GoToSourceCommand.Execute(null));
                return e;
            });

            OnActivation(d =>
            {
                d(vm.Bind(x => x.SelectedFilter, true).Subscribe(x => _viewSegment.SelectedSegment = (nint)x));
                d(_viewSegment.GetChangedObservable().Subscribe(_ => vm.SelectedFilter = (int)_viewSegment.SelectedSegment));
            });
		}
	}
}

