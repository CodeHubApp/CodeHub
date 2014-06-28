using System;
using CodeHub.Core.Services;
using ReactiveUI;
using CodeFramework.Core.ViewModels.Source;
using CodeFramework.Core.Services;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceViewModel : FileSourceViewModel<SourceViewModel.SourceItemModel>
    {
        public IReactiveCommand GoToEditCommand { get; private set; }

        public SourceViewModel(NavObject navObject, IApplicationService applicationService, IAccountsService accountsService)
            : base(navObject)
	    {
            var branch = navObject.Branch;
            var username = navObject.Username;
            var repository = navObject.Repository;
            var trueBranch = navObject.TrueBranch;

            GoToEditCommand = new ReactiveCommand(this.WhenAnyValue(x => x.SourceItem, x => x != null && trueBranch));
	        GoToEditCommand.Subscribe(_ =>
	        {
	            var vm = CreateViewModel<EditSourceViewModel>();
                vm.Path = CurrentItem.Path;
                vm.Branch = branch;
                vm.Username = username;
                vm.Repository = repository;
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

	        LoadCommand.RegisterAsyncTask(async t =>
	        {
                var fileName = System.IO.Path.GetFileName(CurrentItem.Name);
	            if (fileName == null)
	                return;

                var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
                string mime;

                using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    mime = await applicationService.Client.DownloadRawResource2(CurrentItem.GitUrl, stream) ?? string.Empty;
                }

                // We can force a binary representation if it was passed during init. In which case we don't care to figure out via the mime.
                var isBinary = !mime.Contains("charset");
                SourceItem = new FileSourceItemViewModel { FilePath = filepath, IsBinary = (CurrentItem.ForceBinary || isBinary) };
	        });
	    }

        public new class NavObject : FileSourceViewModel<SourceViewModel.SourceItemModel>.NavObject
        {
            public string Branch { get; set; }
            public string Username { get; set; }
            public string Repository { get; set; }
            public bool TrueBranch { get; set; }
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