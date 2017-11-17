using System;
using CodeHub.Core.Services;
using UIKit;
using Splat;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeHub.iOS.DialogElements;
using CodeHub.Core;
using System.Reactive;
using System.Collections.Generic;
using System.Linq;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class TagsViewController : DialogViewController
    {
        private readonly ISubject<Octokit.RepositoryTag> _tagSubject = new Subject<Octokit.RepositoryTag>();

        public IObservable<Octokit.RepositoryTag> TagSelected => _tagSubject.AsObservable();

        public TagsViewController(
            string username,
            string repository,
            IApplicationService applicationService = null)
            : base(UITableViewStyle.Plain)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            var loadTags = ReactiveCommand.CreateFromTask(
                () => applicationService.GitHubClient.Repository.GetAllTags(username, repository));

            loadTags
                .ThrownExceptions
                .Do(_ => TableView.TableFooterView = new UIView())
                .Select(error => new UserError("Unable to load tags.", error))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            loadTags
                .Do(_ => TableView.TableFooterView = null)
                .Subscribe(ItemsLoaded);

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .Do(_ => TableView.TableFooterView = new LoadingIndicatorView())
                .InvokeReactiveCommand(loadTags);
        }

        private void ItemsLoaded(IEnumerable<Octokit.RepositoryTag> tags)
        {
            var items = tags.Select(CreateElement);
            Root.Reset(new Section { items });
        }

        private Element CreateElement(Octokit.RepositoryTag tag)
        {
            var e = new StringElement(tag.Name);
            e.Clicked.Subscribe(_ => _tagSubject.OnNext(tag));
            return e;
        }
    }
}
