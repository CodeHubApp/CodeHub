using System.Reactive.Linq;
using CodeHub.Core.Services;
using System;
using ReactiveUI;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : BaseIssueViewModel
    {
		public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IssueViewModel(
            ISessionService applicationService, 
            IActionMenuFactory actionMenuFactory,
            IMarkdownService markdownService)
            : base(applicationService, markdownService)
        {
            var issuePresenceObservable = this.WhenAnyValue(x => x.Issue).Select(x => x != null);

            this.WhenAnyValue(x => x.Id)
                .Subscribe(x => Title = "Issue #" + x);

            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Issue).Select(x => x != null));
            ShareCommand.Subscribe(_ => actionMenuFactory.ShareUrl(Issue.HtmlUrl));

            GoToEditCommand = ReactiveCommand.Create(issuePresenceObservable);
            GoToEditCommand.Subscribe(_ =>
            {
                var vm = this.CreateViewModel<IssueEditViewModel>();
                vm.RepositoryOwner = RepositoryOwner;
                vm.RepositoryName = RepositoryName;
                vm.Id = Id;
//                vm.Issue = Issue;
//                vm.WhenAnyValue(x => x.Issue).Skip(1).Subscribe(x => Issue = x);
                NavigateTo(vm);
            });

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Issue).Select(x => x != null),
                sender => {
                    var menu = actionMenuFactory.Create(Title);
                    menu.AddButton(Issue.State == Octokit.ItemState.Open ? "Close" : "Open", ToggleStateCommand);
    //
    //
    //                var editButton = _actionSheet.AddButton("Edit");
    //                var commentButton = _actionSheet.AddButton("Comment");
    //                var shareButton = _actionSheet.AddButton("Share");
    //                var showButton = _actionSheet.AddButton("Show in GitHub");

                    return menu.Show(sender);
                });
        }
    }
}

