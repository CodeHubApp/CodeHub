using ReactiveUI;
using CodeHub.Core.Data;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class AccountItemViewModel : ReactiveObject
    {
        private GitHubAccount _account;
        public GitHubAccount Account
        {
            get { return _account; }
            internal set { this.RaiseAndSetIfChanged(ref _account, value); }
        }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            internal set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        public IReactiveCommand<object> DeleteCommand { get; private set; }

        public IReactiveCommand<object> SelectCommand { get; private set; }

        internal AccountItemViewModel()
        {
            DeleteCommand = ReactiveCommand.Create();
            SelectCommand = ReactiveCommand.Create();
        }
    }
}

