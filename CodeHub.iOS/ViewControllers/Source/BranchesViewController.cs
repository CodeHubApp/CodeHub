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
using CodeHub.iOS.Views;
using System.Threading.Tasks;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class BranchesViewController : DialogViewController
    {
        private static string LoadErrorMessage = "Unable to load branches.";
        private readonly ISubject<Octokit.Branch> _branchSubject = new Subject<Octokit.Branch>();

        public IObservable<Octokit.Branch> BranchSelected => _branchSubject.AsObservable();

        private ReactiveCommand<Unit, IReadOnlyList<Octokit.Branch>> LoadBranches { get; }

        public BranchesViewController(
            string username,
            string repository,
            IReadOnlyList<Octokit.Branch> branches = null,
            IApplicationService applicationService = null)
            : base(UITableViewStyle.Plain)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            Title = "Branches";

            LoadBranches = ReactiveCommand.CreateFromTask(() =>
            {
                if (branches != null)
                    return Task.FromResult(branches);
                return applicationService.GitHubClient.Repository.Branch.GetAll(username, repository);
            });

            LoadBranches
                .ThrownExceptions
                .Do(_ => SetErrorView())
                .Select(error => new UserError(LoadErrorMessage, error))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            LoadBranches
                .Do(_ => TableView.TableFooterView = null)
                .Subscribe(ItemsLoaded);

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .Do(_ => TableView.TableFooterView = new LoadingIndicatorView())
                .InvokeReactiveCommand(LoadBranches);
        }

        private void SetErrorView()
        {
            var emptyListView = new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), LoadErrorMessage)
            {
                Alpha = 0
            };

            TableView.TableFooterView = new UIView();
            TableView.BackgroundView = emptyListView;

            UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
                           () => emptyListView.Alpha = 1, null);
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
