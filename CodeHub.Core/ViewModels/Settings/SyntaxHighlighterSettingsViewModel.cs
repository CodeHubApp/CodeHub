using System;
using System.Linq;
using System.Collections.Generic;
using ReactiveUI;
using CodeHub.Core.Services;
using System.Reactive;
using CodeHub.Core.Data;

namespace CodeHub.Core.ViewModels.Settings
{
    public class SyntaxHighlighterViewModel : BaseViewModel
    {
        public IReadOnlyList<string> Themes { get; private set; }

        private string _selectedTheme;
        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set { this.RaiseAndSetIfChanged(ref _selectedTheme, value); }
        }

        public IReactiveCommand<Unit> SaveCommand { get; private set; }

        public SyntaxHighlighterViewModel(ISessionService applicationService,
            IAccountsRepository accountsRepository, IFilesystemService filesystemService)
        {
            Title = "Syntax Highlighter";

            var path = System.IO.Path.Combine("WebResources", "styles");
            Themes = filesystemService.GetFiles(path)
                .Where(x => x.EndsWith(".css", StringComparison.Ordinal))
                .Select(x => System.IO.Path.GetFileNameWithoutExtension(x))
                .ToList();

            SelectedTheme = applicationService.Account.CodeEditTheme ?? "idea";

            SaveCommand = ReactiveCommand.CreateAsyncTask(async t => {
                applicationService.Account.CodeEditTheme = SelectedTheme;
                await accountsRepository.Update(applicationService.Account);
            });
        }
    }
}

