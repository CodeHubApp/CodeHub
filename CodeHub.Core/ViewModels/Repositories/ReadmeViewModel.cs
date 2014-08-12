using System;
using System.Reactive.Linq;
using System.Text;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class ReadmeViewModel : BaseViewModel, ILoadableViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        private string _contentText;
        public string ContentText
        {
            get { return _contentText; }
            set { this.RaiseAndSetIfChanged(ref _contentText, value); }
        }

	    private ContentModel _contentModel;
	    public ContentModel ContentModel
	    {
	        get { return _contentModel; }
	        private set { this.RaiseAndSetIfChanged(ref _contentModel, value); }
	    }

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToGitHubCommand { get; private set; }

        public IReactiveCommand<object> GoToLinkCommand { get; private set; }

        public IReactiveCommand<object> ShareCommand { get; private set; }

        public ReadmeViewModel(IApplicationService applicationService, IMarkdownService markdownService, IShareService shareService)
        {
            ShareCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ContentModel).Select(x => x != null));
            ShareCommand.Subscribe(_ => shareService.ShareUrl(ContentModel.HtmlUrl));

            GoToGitHubCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.ContentModel).Select(x => x != null));
            GoToGitHubCommand.Subscribe(_ => GoToUrlCommand.ExecuteIfCan(ContentModel.HtmlUrl));

            GoToLinkCommand = ReactiveCommand.Create();
            GoToLinkCommand.OfType<string>().Subscribe(x => GoToUrlCommand.ExecuteIfCan(x));

            LoadCommand = ReactiveCommand.CreateAsyncTask(x => 
                this.RequestModel(applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetReadme(), x as bool?, r =>
                {
                    ContentModel = r.Data;
                    var content = Convert.FromBase64String(ContentModel.Content);
                    ContentText = markdownService.Convert(Encoding.UTF8.GetString(content, 0, content.Length));
                }));
        }
    }
}
