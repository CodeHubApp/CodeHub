using System;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.Services;
using Xamarin.Utilities.ViewModels;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Source
{
    public class FileSourceItemViewModel
    {
        public Uri FileUri { get; private set; }
        public bool IsBinary { get; private set; }

        public FileSourceItemViewModel(Uri fileUri, bool binary)
        {
            FileUri = fileUri;
            IsBinary = binary;
        }
    }
        
    public abstract class ContentViewModel : BaseViewModel
    {
        private FileSourceItemViewModel _source;
        public FileSourceItemViewModel SourceItem
		{
            get { return _source; }
            protected set { this.RaiseAndSetIfChanged(ref _source, value); }
		}

        public string Theme { get; private set; }

        public IReactiveCommand<object> OpenWithCommand { get; private set; }

        public IReactiveCommand<object> GoToUrlCommand { get; private set; }

        public abstract bool IsMarkdown { get; }

        public IReactiveCommand<Unit> ShowMenuCommand { get; protected set; }

        protected ContentViewModel(IAccountsService accounts)
        {
            OpenWithCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SourceItem).Select(x => x != null));
            Theme = accounts.ActiveAccount.CodeEditTheme ?? "idea";

            GoToUrlCommand = ReactiveCommand.Create();
            GoToUrlCommand.OfType<string>().Subscribe(x =>
            {
                var vm = this.CreateViewModel<WebBrowserViewModel>();
                vm.Url = x;
                NavigateTo(vm);
            });

        }
    }
}

