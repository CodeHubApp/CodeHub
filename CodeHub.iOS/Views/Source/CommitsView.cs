using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Changesets;
using CodeHub.iOS.DialogElements;
using UIKit;
using CodeHub.iOS.ViewControllers.Repositories;
using System;
using System.Reactive.Linq;
using GitHubSharp.Models;

namespace CodeHub.iOS.Views.Source
{
    public abstract class CommitsView : ViewModelCollectionDrivenDialogViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Commits";

            base.ViewDidLoad();

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;

            var vm = (CommitsViewModel) ViewModel;
            var weakVm = new WeakReference<CommitsViewModel>(vm);
            BindCollection(vm.Commits, x => new CommitElement(x, MakeCallback(weakVm, x)));

            vm.Bind(x => x.ShouldShowPro)
                .Where(x => x)
                .Take(1)
                .Subscribe(_ => this.ShowPrivateView());
        }

        private static Action MakeCallback(WeakReference<CommitsViewModel> weakVm, CommitModel model)
        {
            return new Action(() => weakVm.Get()?.GoToChangesetCommand.Execute(model));
        }
    }
}

