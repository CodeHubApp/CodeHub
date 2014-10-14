using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceViewModel : FileSourceViewModel, ILoadableViewModel
    {
        public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public string Branch { get; set; }

        public string Username { get; set; }

        public string Repository { get; set; }

        public string Path { get; set; }

        public bool ForceBinary { get; set; }

        public string GitUrl { get; set; }

        public string Name { get; set; }

        public string HtmlUrl { get; set; }

        private bool _trueBranch;
        public bool TrueBranch
        {
            get { return _trueBranch; }
            set { this.RaiseAndSetIfChanged(ref _trueBranch, value); }
        }

        public IReactiveCommand LoadCommand { get; private set; }

        public SourceViewModel(IApplicationService applicationService, IAccountsService accountsService, IFilesystemService filesystemService)
            : base(accountsService)
	    {
            GoToEditCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SourceItem, x => x.TrueBranch).Select(x => x.Item1 != null && x.Item2));
	        GoToEditCommand.Subscribe(_ =>
	        {
	            var vm = CreateViewModel<EditSourceViewModel>();
                vm.Path = Path;
                vm.Branch = Branch;
                vm.Username = Username;
                vm.Repository = Repository;
	            ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
	            string filepath;
                string mime;

                using (var stream = filesystemService.CreateTempFile(out filepath))
                {
                    mime = await applicationService.Client.DownloadRawResource2(GitUrl, stream) ?? string.Empty;
                }

                // We can force a binary representation if it was passed during init. In which case we don't care to figure out via the mime.
                var isBinary = !mime.Contains("charset");
                var fileUri = new Uri(filepath);
                SourceItem = new FileSourceItemViewModel(fileUri, ForceBinary || isBinary);
	        });
	    }

        public class SourceItemModel
        {
            public bool ForceBinary { get; set; }
            public string Path { get; set; }
            public string GitUrl { get; set; }
            public string Name { get; set; }
            public string HtmlUrl { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != typeof(SourceItemModel))
                    return false;
                SourceItemModel other = (SourceItemModel)obj;
                return GitUrl == other.GitUrl;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (GitUrl != null ? GitUrl.GetHashCode() : 0);
                }
            }
        }
    }
}