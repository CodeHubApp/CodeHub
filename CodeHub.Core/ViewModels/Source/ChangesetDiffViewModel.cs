using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.ViewModels;
using Xamarin.Utilities.Services;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Source
{
    public class ChangesetDiffViewModel : ContentViewModel, ILoadableViewModel
    {
        private CommitModel.CommitFileModel _commitFile;
        private string _filename;
		private string _actualFilename;

		public string Username { get; set; }

		public string Repository { get; set; }

		public string Branch { get; set; }

	    public string Filename
	    {
	        get { return _filename; }
	        set { this.RaiseAndSetIfChanged(ref _filename, value); }
	    }

	    public CommitModel.CommitFileModel CommitFile
	    {
	        get { return _commitFile; }
	        set { this.RaiseAndSetIfChanged(ref _commitFile, value); }
	    }

		public ReactiveList<CommentModel> Comments { get; private set; }

        public IReactiveCommand<object> GoToCommentCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public override bool IsMarkdown 
        {
            get { return false; }
        }

	    public ChangesetDiffViewModel(IAccountsService accounts, IApplicationService applicationService, IFilesystemService filesystemService)
            : base(accounts)
	    {
            Comments = new ReactiveList<CommentModel>();

            GoToCommentCommand = ReactiveCommand.Create();
            GoToCommentCommand.OfType<int?>().Subscribe(line =>
            {
                var vm = this.CreateViewModel<CommentViewModel>();
                ReactiveUI.Legacy.ReactiveCommandMixins.RegisterAsyncTask(vm.SaveCommand, async t =>
                {
                    var req = applicationService.Client.Users[Username].Repositories[Repository].Commits[Branch].Comments.Create(vm.Comment, Filename, line);
                    var response = await applicationService.Client.ExecuteAsync(req);
			        Comments.Add(response.Data);
                    Dismiss();
                });
                NavigateTo(vm);
            });

	        this.WhenAnyValue(x => x.Filename).Subscribe(x =>
	        {
	            if (string.IsNullOrEmpty(x))
                    Title = "Diff";
	            else
	            {
	                _actualFilename = Path.GetFileName(Filename) ??
	                                  Filename.Substring(Filename.LastIndexOf('/') + 1);
                    Title = _actualFilename;
	            }
	        });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
	            var branch = applicationService.Client.Users[Username].Repositories[Repository].Commits[Branch];

	            //Make sure we have this information. If not, go get it
			    if (CommitFile == null)
			    {
				    var data = await applicationService.Client.ExecuteAsync(branch.Get());
                    CommitFile = data.Data.Files.First(x => string.Equals(x.Filename, Filename));
			    }

	            string path;
	            using (var stream = filesystemService.CreateTempFile(out path))
	            {
	                using (var fs = new StreamWriter(stream))
	                {
	                    fs.Write(CommitFile.Patch);
	                }
	            }

                //SourceItem = new FileSourceItemViewModel { FilePath = path };
			    await Comments.SimpleCollectionLoad(branch.Comments.GetAll(), t as bool?);

	        });
	    }
    }
}

