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

        public IReactiveCommand<bool> DismissCommand { get; private set; }

        public GistCommentViewModel(ISessionService applicationService, IAlertDialogFactory alertDialogFactory) 
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

