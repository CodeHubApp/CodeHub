using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistCommentViewModel : BaseViewModel, IComposerViewModel
    {
        public string Id { get; set; }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public IReactiveCommand<GitHubSharp.Models.GistCommentModel> SaveCommand { get; protected set; }

        public GistCommentViewModel(IApplicationService applicationService) 
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                async t => 
                {
                    var request = applicationService.Client.Gists[Id].CreateGistComment(Text);
                    return (await applicationService.Client.ExecuteAsync(request)).Data;
                });
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

