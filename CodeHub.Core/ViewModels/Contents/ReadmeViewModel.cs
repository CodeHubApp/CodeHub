using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Contents
{
    public class ReadmeViewModel : BaseViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private string _contentText;
        public string ContentText
        {
            get { return _contentText; }
            private set { this.RaiseAndSetIfChanged(ref _contentText, value); }
        }

	    private ContentModel _contentModel;
	    public ContentModel ContentModel
	    {
	        get { return _contentModel; }
	        private set { this.RaiseAndSetIfChanged(ref _contentModel, value); }
	    }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToGitHubCommand { get; private set; }

        public IReactiveCommand<object> GoToLinkCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; private set; }

        public ReadmeViewModel(IApplicationService applicationService, 
            IShareService shareService, IActionMenuFactory actionMenuService)
        {
            Title = "Readme";

            var nonNullContentModel = this.WhenAnyValue(x => x.ContentModel).Select(x => x != null);

            ShareCommand = ReactiveCommand.Create(nonNullContentModel);
            ShareCommand.Subscribe(_ => shareService.ShareUrl(new Uri(ContentModel.HtmlUrl)));

            var showWebBrowser = new Action<string>(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });

            GoToGitHubCommand = ReactiveCommand.Create(nonNullContentModel);
            GoToGitHubCommand.Select(_ => ContentModel.HtmlUrl).Subscribe(showWebBrowser);

            GoToLinkCommand = ReactiveCommand.Create();
            GoToLinkCommand.OfType<string>().Subscribe(showWebBrowser);

            ShowMenuCommand = ReactiveCommand.CreateAsyncTask(nonNullContentModel, _ =>
            {
                var menu = actionMenuService.Create(Title);
                menu.AddButton("Share", ShareCommand);
                menu.AddButton("Show in GitHub", GoToGitHubCommand);
                return menu.Show();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async x =>
            {
                var repository = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName];
                ContentText = await repository.GetReadmeRendered();
                ContentModel = (await applicationService.Client.ExecuteAsync(repository.GetReadme())).Data;
            });
        }
    }
}
