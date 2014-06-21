using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitMessageViewModel : BaseViewModel
    {
        private string _message;

        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }

        public IReactiveCommand SaveCommand { get; private set; }

        public CommitMessageViewModel()
        {
            SaveCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Message, x => !string.IsNullOrEmpty(x)));
        }
    }
}