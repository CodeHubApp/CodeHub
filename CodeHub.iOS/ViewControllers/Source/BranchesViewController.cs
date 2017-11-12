using System;
using CodeHub.Core.Services;
using UIKit;
using Splat;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeHub.iOS.DialogElements;
using System.Collections.Generic;
using CodeHub.Core;
using System.Linq;
using System.Reactive;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class BranchesViewController : DialogViewController
    {
        private readonly ISubject<Octokit.Branch> _branchSubject = new Subject<Octokit.Branch>();

        public IObservable<Octokit.Branch> BranchSelected => _branchSubject.AsObservable();

        public BranchesViewController(
            string username,
            string repository,
            IApplicationService applicationService = null)
            : base(UITableViewStyle.Plain)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            var loadBranches = ReactiveCommand.CreateFromTask(
                () => applicationService.GitHubClient.Repository.Branch.GetAll(username, repository));

            loadBranches
                .ThrownExceptions
                .Select(error => new UserError("Unable to load branches: " + error.Message))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            loadBranches.Subscribe(ItemsLoaded);

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(loadBranches);
        }

        private void ItemsLoaded(IEnumerable<Octokit.Branch> branches)
        {
            var items = branches.Select(CreateElement);
            Root.Reset(new Section { items });
        }

        private Element CreateElement(Octokit.Branch branch)
        {
            var e = new StringElement(branch.Name);
            e.Clicked.Subscribe(_ => _branchSubject.OnNext(branch));
            return e;
        }
    }
}
