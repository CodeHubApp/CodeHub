using System;
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
    public class TagsViewController : ListViewController<RepositoryTag>
    {
        private readonly ISubject<RepositoryTag> _tagSubject = new Subject<RepositoryTag>();

        public IObservable<RepositoryTag> TagSelected => _tagSubject.AsObservable();

        private static EmptyListView CreateEmptyListView()
            => new EmptyListView(Octicon.Tag.ToEmptyListImage(), "There are no tags!");

        public static TagsViewController FromGitHub(
            string username,
            string repository,
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            var uri = ApiUrls.RepositoryTags(username, repository);
            var dataRetriever = new GitHubList<RepositoryTag>(applicationService.GitHubClient, uri);
            return new TagsViewController(dataRetriever);
        }

        public TagsViewController(IDataRetriever<RepositoryTag> dataRetriever)
            : base(dataRetriever, CreateEmptyListView)
        {
            Title = "Tags";
        }

        protected override Element ConvertToElement(RepositoryTag item)
        {
            var e = new StringElement(item.Name);
            e.Clicked.Select(_ => item).Subscribe(_tagSubject.OnNext);
            return e;
        }
    }
}
