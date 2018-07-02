using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeHub.iOS.DialogElements;
using Octokit;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class TagsViewController : GitHubListViewController<RepositoryTag>
    {
        private readonly ISubject<RepositoryTag> _tagSubject = new Subject<RepositoryTag>();

        public IObservable<RepositoryTag> TagSelected => _tagSubject.AsObservable();

        public TagsViewController(string username, string repository)
            : base(ApiUrls.RepositoryTags(username, repository))
        {
            Title = "Tags";
        }

        protected override Element ConvertToElement(RepositoryTag item)
        {
            var e = new StringElement(item.Name);
            e.Clicked.Subscribe(_ => _tagSubject.OnNext(item));
            return e;
        }
    }
}
