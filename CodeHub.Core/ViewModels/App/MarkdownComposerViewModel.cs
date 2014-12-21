using System;
using System.Reactive.Linq;
using ReactiveUI;
using System.Threading.Tasks;
using Xamarin.Utilities.ViewModels;
using System.Reactive;

namespace CodeHub.Core.ViewModels.App
{
    public abstract class MarkdownComposerViewModel : BaseViewModel, IComposerViewModel
    {
        public IReactiveCommand<Unit> SaveCommand { get; protected set; }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        protected MarkdownComposerViewModel()
        {
            var saveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => Save());
            saveCommand.Subscribe(x => Dismiss());
            SaveCommand = saveCommand;
        }

        protected abstract Task Save();
    }
}