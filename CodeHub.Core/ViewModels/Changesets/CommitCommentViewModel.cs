using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;

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

        public CommitCommentViewModel(IApplicationService applicationService) 
        {
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                async t => 
                {
                    var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Commits[Node].Comments.Create(Text);
                    return (await applicationService.Client.ExecuteAsync(request)).Data;
                });
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

