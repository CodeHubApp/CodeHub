using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
    public class CommentViewModel : BaseViewModel
    {
        private string _comment;

        public string Comment
        {
            get { return _comment; }
            private set { this.RaiseAndSetIfChanged(ref _comment, value); }
        }

        public IReactiveCommand SaveCommand { get; private set; }

        public CommentViewModel()
        {
            SaveCommand = new ReactiveCommand(this.WhenAnyValue(x => x.Comment, x => !string.IsNullOrEmpty(x)));
        }
    }
}