using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueCommentViewModel : BaseViewModel, IComposerViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int Id { get; set; }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public IReactiveCommand<GitHubSharp.Models.IssueCommentModel> SaveCommand { get; private set; }

        public IssueCommentViewModel(ISessionService applicationService) 
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                async t => {
                var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].Issues[Id].CreateComment(Text);
                return (await applicationService.Client.ExecuteAsync(request)).Data;
            });
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

