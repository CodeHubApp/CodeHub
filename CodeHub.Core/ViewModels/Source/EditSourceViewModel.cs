using System;
using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Source
{
    public class EditSourceViewModel : BaseViewModel, ILoadableViewModel
    {
        private ISubject<ContentUpdateModel> _sourceChangedSubject = new Subject<ContentUpdateModel>();

		private string _text;
		public string Text
		{
			get { return _text; }
			set { this.RaiseAndSetIfChanged(ref _text, value); }
		}

		public string Username { get; set; }

		public string Repository { get; set; }

        private string _path;
		public string Path 
        {
            get { return _path; }
            set { this.RaiseAndSetIfChanged(ref _path, value); }
        }

		public string BlobSha { get; set; }

		public string Branch { get; set; }

        public IObservable<ContentUpdateModel> SourceChanged
        {
            get { return _sourceChangedSubject; }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> SaveCommand { get; private set; }

        public EditSourceViewModel(IApplicationService applicationService)
	    {
            Title = "Edit";

            var canSave = this.WhenAnyValue(x => x.Text).Select(x => x != null);
            SaveCommand = ReactiveCommand.Create(canSave).WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<CommitMessageViewModel>();
                vm.Username = Username;
                vm.Repository = Repository;
                vm.Path = Path;
                vm.CommitMessage = "Updated " + Path.Substring(Path.LastIndexOf('/') + 1);
                vm.Branch = Branch;
                vm.BlobSha = BlobSha;
                vm.Saved.Subscribe(_sourceChangedSubject.OnNext);
                vm.Text = Text;
                NavigateTo(vm);
            });

            SourceChanged.Subscribe(x => Dismiss());

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
                Text = System.Text.Encoding.UTF8.GetString(content, 0, content.Length) ?? string.Empty;
	        });
	    }
    }
}

