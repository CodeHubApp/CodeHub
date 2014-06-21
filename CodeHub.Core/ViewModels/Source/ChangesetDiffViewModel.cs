using System;
using System.Linq;
using System.Reactive.Linq;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using GitHubSharp.Models;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;

namespace CodeHub.Core.ViewModels.Source
{
	public class ChangesetDiffViewModel : FileSourceViewModel
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

		public ReactiveCollection<CommentModel> Comments { get; private set; }

        public IReactiveCommand GoToCommentCommand { get; private set; }

	    public ChangesetDiffViewModel(IApplicationService applicationService)
	    {
            Comments = new ReactiveCollection<CommentModel>();

            GoToCommentCommand = new ReactiveCommand();
            GoToCommentCommand.OfType<int?>().Subscribe(line =>
            {
                var vm = CreateViewModel<CommentViewModel>();
                vm.SaveCommand.RegisterAsyncTask(async t =>
                {
                    var req = applicationService.Client.Users[Username].Repositories[Repository].Commits[Branch].Comments.Create(vm.Comment, Filename, line);
                    var response = await applicationService.Client.ExecuteAsync(req);
			        Comments.Add(response.Data);
                    vm.DismissCommand.ExecuteIfCan();
                });
                ShowViewModel(vm);
            });

	        this.WhenAnyValue(x => x.Filename).Subscribe(x =>
	        {
	            if (string.IsNullOrEmpty(x))
	                Title = "Diff";
	            else
	            {
	                _actualFilename = System.IO.Path.GetFileName(Filename) ??
	                                  Filename.Substring(Filename.LastIndexOf('/') + 1);
	                Title = _actualFilename;
	            }
	        });

	        LoadCommand.RegisterAsyncTask(async t =>
	        {
	            var branch = applicationService.Client.Users[Username].Repositories[Repository].Commits[Branch];

	            //Make sure we have this information. If not, go get it
			    if (CommitFile == null)
			    {
				    var data = await applicationService.Client.ExecuteAsync(branch.Get());
                    CommitFile = data.Data.Files.First(x => string.Equals(x.Filename, Filename));
			    }

                FilePath = CreatePlainContentFile(CommitFile.Patch, _actualFilename);
			    await Comments.SimpleCollectionLoad(branch.Comments.GetAll(), t as bool?);

	        });
	    }
    }
}

