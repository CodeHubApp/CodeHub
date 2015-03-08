using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Factories;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitCommentViewModel : BaseViewModel, IComposerViewModel
    {
        public string Node { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public IReactiveCommand<GitHubSharp.Models.CommentModel> SaveCommand { get; protected set; }

        public IReactiveCommand<bool> DismissCommand { get; private set; }

        public CommitCommentViewModel(ISessionService applicationService, IAlertDialogFactory alertDialogFactory) 
        {
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                async t => 
                {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Comments.Create(Text);
                    return (await applicationService.Client.ExecuteAsync(request)).Data;
                });
            SaveCommand.Subscribe(x => Dismiss());

            DismissCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                if (string.IsNullOrEmpty(Text))
                    return true;
                return await alertDialogFactory.PromptYesNo("Discard Comment?", "Are you sure you want to discard this comment?");
            });
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
        }
    }
}

