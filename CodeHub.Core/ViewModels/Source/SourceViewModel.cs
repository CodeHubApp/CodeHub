using System;
using CodeHub.Core.Services;
using ReactiveUI;
using CodeFramework.Core.ViewModels.Source;
using CodeFramework.Core.Services;
using System.Reactive.Linq;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceViewModel : FileSourceViewModel<SourceViewModel.SourceItemModel>
    {
        public IReactiveCommand<object> GoToEditCommand { get; private set; }

        public string Branch { get; set; }

        public string Username { get; set; }

        public string Repository { get; set; }

        private bool _trueBranch;
        public bool TrueBranch
        {
            get { return _trueBranch; }
            set { this.RaiseAndSetIfChanged(ref _trueBranch, value); }
        }

        private readonly IReactiveCommand _loadCommand;
        public override IReactiveCommand LoadCommand
        {
            get { return _loadCommand; }
        }

        public SourceViewModel(IApplicationService applicationService, IAccountsService accountsService, IFilesystemService filesystemService)
	    {
            GoToEditCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SourceItem, x => x.TrueBranch).Select(x => x.Item1 != null && x.Item2));
	        GoToEditCommand.Subscribe(_ =>
	        {
	            var vm = CreateViewModel<EditSourceViewModel>();
                vm.Path = CurrentItem.Path;
                vm.Branch = Branch;
                vm.Username = Username;
                vm.Repository = Repository;
	            ShowViewModel(vm);
            });

            Theme = applicationService.Account.CodeEditTheme;
            this.WhenAnyValue(x => x.Theme).Skip(1).Subscribe(x =>
            {
                applicationService.Account.CodeEditTheme = x;
                accountsService.Update(applicationService.Account);
            });

            this.WhenAnyValue(x => x.CurrentItem)
                .Skip(1)
                .Where(x => x != null)
                .Subscribe(_ => LoadCommand.ExecuteIfCan());

            _loadCommand = ReactiveCommand.CreateAsyncTask(async t =>
	        {
                var fileName = System.IO.Path.GetFileName(CurrentItem.Name);
	            if (fileName == null)
	                return;

	            string filepath;
                string mime;

                using (var stream = filesystemService.CreateTempFile(out filepath))
                {
                    mime = await applicationService.Client.DownloadRawResource2(CurrentItem.GitUrl, stream) ?? string.Empty;
                }

                // We can force a binary representation if it was passed during init. In which case we don't care to figure out via the mime.
                var isBinary = !mime.Contains("charset");
                SourceItem = new FileSourceItemViewModel { FilePath = filepath, IsBinary = (CurrentItem.ForceBinary || isBinary) };
	        });

            SetupRx();
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