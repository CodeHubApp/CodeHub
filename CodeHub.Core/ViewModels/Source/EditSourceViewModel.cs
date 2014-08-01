using System;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class EditSourceViewModel : BaseViewModel, ILoadableViewModel
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

        public IReactiveCommand LoadCommand { get; private set; }

        public IReactiveCommand<object> SaveCommand { get; private set; }

	    public EditSourceViewModel(IApplicationService applicationService)
	    {
            SaveCommand = ReactiveCommand.Create();
	        SaveCommand.Subscribe(_ =>
	        {
	            var vm = CreateViewModel<CommitMessageViewModel>();
                vm.Username = Username;
                vm.Repository = Repository;
                vm.Path = Path;
                vm.Text = Text;
                vm.BlobSha = BlobSha;
                vm.Branch = Branch;
                vm.ContentChanged.Subscribe(x => Content = x);
                ShowViewModel(vm);
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
	            var path = Path;
                if (!path.StartsWith("/", StringComparison.Ordinal))
                    path = "/" + path;

	            var request = applicationService.Client.Users[Username].Repositories[Repository].GetContentFile(path, Branch ?? "master");
			    request.UseCache = false;
			    var data = await applicationService.Client.ExecuteAsync(request);
			    BlobSha = data.Data.Sha;
	            var content = Convert.FromBase64String(data.Data.Content);
                Text = System.Text.Encoding.UTF8.GetString(content, 0, content.Length);
	        });
	    }
    }
}

