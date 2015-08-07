using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Contents
{
    public class EditFileViewModel : BaseViewModel, ILoadableViewModel
    {
        private DateTime _lastLoad, _lastEdit;

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        public string BlobSha { get; private set; }

        public string Branch { get; private set; }

		private string _text;
		public string Text
		{
			get { return _text; }
			set { this.RaiseAndSetIfChanged(ref _text, value); }
		}

        private string _commitMessage;
        public string CommitMessage
        {
            get { return _commitMessage; }
            set { this.RaiseAndSetIfChanged(ref _commitMessage, value); }
        }

        private string _path;
		public string Path 
        {
            get { return _path; }
            private set { this.RaiseAndSetIfChanged(ref _path, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; }

        public IReactiveCommand<Octokit.RepositoryContentChangeSet> SaveCommand { get; }

        public IReactiveCommand<bool> DismissCommand { get; }

        public EditFileViewModel(ISessionService sessionService, IAlertDialogFactory alertDialogFactory)
	    {
            Title = "Edit";

            this.WhenAnyValue(x => x.Path)
                .IsNotNull()
                .Subscribe(x => CommitMessage = "Updated " + x.Substring(x.LastIndexOf('/') + 1));

            this.WhenAnyValue(x => x.Text)
                .Subscribe(x => _lastEdit = DateTime.Now);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
	            var path = Path;
                if (!path.StartsWith("/", StringComparison.Ordinal))
                    path = "/" + path;

	            var request = sessionService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContentFile(path, Branch ?? "master");
			    request.UseCache = false;
			    var data = await sessionService.Client.ExecuteAsync(request);
			    BlobSha = data.Data.Sha;
	            var content = Convert.FromBase64String(data.Data.Content);
                Text = System.Text.Encoding.UTF8.GetString(content, 0, content.Length) ?? string.Empty;
                _lastLoad = DateTime.Now;
	        });

            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CommitMessage).Select(x => !string.IsNullOrEmpty(x)), async _ => {
                    var path = Path.TrimStart('/');
                    var request = new Octokit.UpdateFileRequest(CommitMessage, Text, BlobSha) { Branch = Branch };
                    using (alertDialogFactory.Activate("Commiting..."))
                        return await sessionService.GitHubClient.Repository.Content.UpdateFile(RepositoryOwner, RepositoryName, path, request);
            });
            SaveCommand.Subscribe(x => Dismiss());

            DismissCommand = ReactiveCommand.CreateAsyncTask(async t => {
                if (string.IsNullOrEmpty(Text)) return true;
                if (_lastEdit <= _lastLoad) return true;
                return await alertDialogFactory.PromptYesNo("Discard Edit?", "Are you sure you want to discard these changes?");
            });
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
	    }

        public EditFileViewModel Init(string repositoryOwner, string repositoryName, string path, string blobSha, string branch)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            Path = path;
            BlobSha = blobSha;
            Branch = branch;
            return this;
        }
    }
}

