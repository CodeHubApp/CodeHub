using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.DialogElements;
using System;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetBranchesView : ViewModelCollectionDrivenDialogViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Changeset Branch";
            NoItemsText = "No Branches";

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

