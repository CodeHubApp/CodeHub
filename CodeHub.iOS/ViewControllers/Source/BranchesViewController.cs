using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;
using Octokit;
using Splat;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class BranchesViewController : ListViewController<Branch>
    {
        private readonly ISubject<Branch> _branchSubject = new Subject<Branch>();

        public IObservable<Branch> BranchSelected => _branchSubject.AsObservable();

        private static EmptyListView CreateEmptyListView()
            => new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), "There are no branches!");

        public static BranchesViewController FromStaticList(IEnumerable<Branch> branches)
            => new BranchesViewController(StaticList.From(branches));

        public static BranchesViewController FromGitHub(
            string username, 
            string repository,
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            var uri = ApiUrls.RepoBranches(username, repository);
            var dataRetriever = new GitHubList<Branch>(applicationService.GitHubClient, uri);
            return new BranchesViewController(dataRetriever);
        }

        private BranchesViewController(IDataRetriever<Branch> dataRetriever)
            : base(dataRetriever, CreateEmptyListView)
        {
            Title = "Branches";
        }

        protected override Element ConvertToElement(Branch item)
        {
            var e = new StringElement(item.Name);
            e.Clicked.Subscribe(_ => _branchSubject.OnNext(item));
            return e;
        }
    }
}
