using System;
using System.Reactive.Linq;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.Services;

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
        
    public abstract class FileSourceViewModel : BaseViewModel
    {
        private FileSourceItemViewModel _source;
        public FileSourceItemViewModel SourceItem
		{
            get { return _source; }
            protected set { this.RaiseAndSetIfChanged(ref _source, value); }
		}

        private string _theme;
        public string Theme
        {
            get { return _theme; }
            set { this.RaiseAndSetIfChanged(ref _theme, value); }
        }

        protected FileSourceViewModel(IAccountsService accounts)
        {
            Theme = accounts.ActiveAccount.CodeEditTheme ?? "idea";
            this.WhenAnyValue(x => x.Theme).Skip(1).Subscribe(x =>
            {
                accounts.ActiveAccount.CodeEditTheme = x;
                accounts.Update(accounts.ActiveAccount);
            });
        }
    }
}

