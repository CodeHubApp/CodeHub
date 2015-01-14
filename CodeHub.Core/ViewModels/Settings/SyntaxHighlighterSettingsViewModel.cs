using System;
using Xamarin.Utilities.Services;
using System.Linq;
using System.Collections.Generic;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Settings
{
    public class SyntaxHighlighterSettingsViewModel : BaseViewModel
    {
        public IReadOnlyList<string> Themes { get; private set; }

        private string _selectedTheme;
        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set { this.RaiseAndSetIfChanged(ref _selectedTheme, value); }
        }

        public IReactiveCommand<Unit> SaveCommand { get; private set; }

        public SyntaxHighlighterSettingsViewModel(IAccountsService accountsService, IFilesystemService filesystemService)
        {
            Title = "Syntax Highlighter";

            var path = System.IO.Path.Combine("WebResources", "styles");
            Themes = filesystemService.GetFiles(path)
                .Where(x => x.EndsWith(".css", StringComparison.Ordinal))
                .Select(x => System.IO.Path.GetFileNameWithoutExtension(x))
                .ToList();

            SelectedTheme = accountsService.ActiveAccount.CodeEditTheme ?? "idea";

            SaveCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                accountsService.ActiveAccount.CodeEditTheme = SelectedTheme;
                accountsService.Update(accountsService.ActiveAccount);
            });
        }
    }
}

