using System;
using CodeHub.Core.Services;
using ReactiveUI;
using CodeFramework.Core.ViewModels.Source;

namespace CodeHub.Core.ViewModels.Source
{
	public class SourceViewModel : FileSourceViewModel
    {
        private bool _trueBranch;
        private string _path;

		public string Username { get; set; }

		public string Repository { get; set; }

		public string Branch { get; set; }

        public bool ForceBinary { get; set; }

        public string GitUrl { get; set; }

        public string Name { get; set; }

	    public string Path
	    {
	        get { return _path; }
	        set { this.RaiseAndSetIfChanged(ref _path, value); }
	    }

	    public bool TrueBranch
	    {
	        get { return _trueBranch; }
	        set { this.RaiseAndSetIfChanged(ref _trueBranch, value); }
	    }

        public IReactiveCommand GoToEditCommand { get; private set; }

	    public SourceViewModel(IApplicationService applicationService)
	    {
            GoToEditCommand = new ReactiveCommand(this.WhenAnyValue(x => x.SourceItem, y => y.TrueBranch, (x, y) => x != null && y));
	        GoToEditCommand.Subscribe(_ =>
	        {
	            var vm = CreateViewModel<EditSourceViewModel>();
	            vm.Path = Path;
	            vm.Branch = Branch;
	            vm.Username = Username;
	            vm.Repository = Repository;
	            ShowViewModel(vm);
	        });

	        LoadCommand.RegisterAsyncTask(async t =>
	        {
	            var fileName = System.IO.Path.GetFileName(Name);
	            if (fileName == null)
	                return;

                var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
                string mime;

                using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    mime = await applicationService.Client.DownloadRawResource2(GitUrl, stream) ?? string.Empty;
                }

                // We can force a binary representation if it was passed during init. In which case we don't care to figure out via the mime.
                var isBinary = !mime.Contains("charset");
                SourceItem = new SourceItemViewModel { FilePath = filepath, IsBinary = (ForceBinary || isBinary) };
	        });
	    }
    }
}