using ReactiveUI;
using System.Reactive.Linq;
using Xamarin.Utilities.ViewModels;

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

        public ReactiveUI.Legacy.ReactiveCommand SaveCommand { get; private set; }

        public CommentViewModel()
        {
            SaveCommand = new ReactiveUI.Legacy.ReactiveCommand(this.WhenAnyValue(x => x.Comment).Select(x => !string.IsNullOrEmpty(x)));
        }
    }
}