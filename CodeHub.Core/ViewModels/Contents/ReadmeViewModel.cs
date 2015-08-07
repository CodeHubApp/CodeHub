using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Factories;
using Octokit;

namespace CodeHub.Core.ViewModels.Contents
{
    public class ReadmeViewModel : BaseViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        private string _contentText;
        public string ContentText
        {
            get { return _contentText; }
            private set { this.RaiseAndSetIfChanged(ref _contentText, value); }
        }

        private Readme _contentModel;
        public Readme ContentModel
	    {
	        get { return _contentModel; }
	        private set { this.RaiseAndSetIfChanged(ref _contentModel, value); }
	    }

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReactiveCommand<object> GoToGitHubCommand { get; }

        public IReactiveCommand<object> GoToLinkCommand { get; }

        public IReactiveCommand<object> ShareCommand { get; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; }

        public ReadmeViewModel(
            ISessionService applicationService, 
            IActionMenuFactory actionMenuService)
        {
            Title = "Readme";

            var nonNullContentModel = this.WhenAnyValue(x => x.ContentModel).Select(x => x != null);

            ShareCommand = ReactiveCommand.Create(nonNullContentModel);
            ShareCommand.Subscribe(sender => actionMenuService.ShareUrl(sender, ContentModel.HtmlUrl));

            GoToGitHubCommand = ReactiveCommand.Create(nonNullContentModel);
            GoToGitHubCommand.Select(_ => ContentModel.HtmlUrl).Subscribe(GoToWebBrowser);

            GoToLinkCommand = ReactiveCommand.Create();
            GoToLinkCommand.OfType<string>().Subscribe(x => GoToWebBrowser(new Uri(x)));

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(nonNullContentModel, sender => {
                var menu = actionMenuService.Create();
                menu.AddButton("Share", ShareCommand);
                menu.AddButton("Show in GitHub", GoToGitHubCommand);
                return menu.Show(sender);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async x => {
                ContentText = await applicationService.GitHubClient.Repository.Content.GetReadmeHtml(RepositoryOwner, RepositoryName);
                ContentModel = await applicationService.GitHubClient.Repository.Content.GetReadme(RepositoryOwner, RepositoryName);
            });
        }

        private void GoToWebBrowser(Uri uri)
        {
            var vm = this.CreateViewModel<WebBrowserViewModel>();
            vm.Init(uri);
            NavigateTo(vm);
        }

        public ReadmeViewModel Init(string repositoryOwner, string repositoryName)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            return this;
        }
    }
}
