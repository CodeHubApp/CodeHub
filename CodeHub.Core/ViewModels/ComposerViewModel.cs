using System;
using System.Threading.Tasks;
using CodeHub.Core.Factories;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels
{
    public class ComposerViewModel : BaseViewModel, IComposerViewModel
    {
        public string Id { get; set; }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public IReactiveCommand<Unit> SaveCommand { get; protected set; }

        public IReactiveCommand<bool> DismissCommand { get; private set; }

        public ComposerViewModel(Func<string, Task> saveFunc, IAlertDialogFactory alertDialogFactory) 
            : this("Add Comment", saveFunc, alertDialogFactory)
        {
        }

        public ComposerViewModel(string title, Func<string, Task> saveFunc, IAlertDialogFactory alertDialogFactory) 
        {
            Title = title;
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => saveFunc(Text));
            SaveCommand.AlertExecuting(alertDialogFactory, "Saving...");
            SaveCommand.Subscribe(x => Dismiss());

            DismissCommand = ReactiveCommand.CreateAsyncTask(async t =>
                {
                    if (string.IsNullOrEmpty(Text))
                        return true;
                    return await alertDialogFactory.PromptYesNo("Discard Comment?", "Are you sure you want to discard this comment?");
                });
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
        }
    }
}

