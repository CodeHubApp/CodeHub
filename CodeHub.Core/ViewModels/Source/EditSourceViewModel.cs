using System;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
	public class EditSourceViewModel : LoadableViewModel
    {
		private string _text;
		public string Text
		{
			get { return _text; }
			private set { this.RaiseAndSetIfChanged(ref _text, value); }
		}

	    private ContentUpdateModel _content;
	    public ContentUpdateModel Content
	    {
	        get { return _content; }
	        private set { this.RaiseAndSetIfChanged(ref _content, value); }
	    }

		public string Username { get; set; }

		public string Repository { get; set; }

		public string Path { get; set; }

		public string BlobSha { get; set; }

		public string Branch { get; set; }

        public IReactiveCommand SaveCommand { get; private set; }

	    public EditSourceViewModel(IApplicationService applicationService)
	    {
            SaveCommand = new ReactiveCommand();
	        SaveCommand.Subscribe(_ =>
	        {
	            var vm = CreateViewModel<CommitMessageViewModel>();
	            vm.SaveCommand.RegisterAsyncTask(async t =>
	            {
                    var request = applicationService.Client.Users[Username].Repositories[Repository]
                        .UpdateContentFile(Path, vm.Message, Text, BlobSha, Branch);
                    var response = await applicationService.Client.ExecuteAsync(request);
	                Content = response.Data;
                    DismissCommand.ExecuteIfCan();
	            });
                ShowViewModel(vm);
	        });

	        LoadCommand.RegisterAsyncTask(async t =>
	        {
	            var path = Path;
                if (!path.StartsWith("/", StringComparison.Ordinal))
                    path = "/" + path;

	            var request = applicationService.Client.Users[Username].Repositories[Repository].GetContentFile(path, Branch ?? "master");
			    request.UseCache = false;
			    var data = await applicationService.Client.ExecuteAsync(request);
			    BlobSha = data.Data.Sha;
			    Text = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data.Data.Content));
	        });
	    }
    }
}

