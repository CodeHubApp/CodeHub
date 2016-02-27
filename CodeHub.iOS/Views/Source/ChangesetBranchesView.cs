using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.DialogElements;
using System;
using UIKit;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetBranchesView : ViewModelCollectionDrivenDialogViewController
    {
        public ChangesetBranchesView()
        {
            Title = "Changeset Branch";
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), "There are no branches."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (ChangesetBranchesViewModel) ViewModel;
            var weakVm = new WeakReference<ChangesetBranchesViewModel>(vm);
            BindCollection(vm.Branches, x => {
                var e = new StringElement(x.Name);
                e.Clicked.Subscribe(_ => weakVm.Get()?.GoToBranchCommand.Execute(x));
                return e;
            });
        }
    }
}

