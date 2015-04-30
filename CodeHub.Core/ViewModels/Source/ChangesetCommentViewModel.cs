using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Source
{
    public class ChangesetCommentViewModel : BaseViewModel, IComposerViewModel
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public IReactiveCommand<Unit> SaveCommand { get; private set; }

        public IReactiveCommand<bool> DismissCommand { get; private set; }

        public ChangesetCommentViewModel(IAlertDialogFactory alertDialogFactory, Func<Task> saveFunc)
        {
            Title = "Comment";

            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                async t => {
                    await saveFunc();
                    Dismiss();
                });

            DismissCommand = ReactiveCommand.CreateAsyncTask(async t => {
                if (string.IsNullOrEmpty(Text))
                    return true;
                return await alertDialogFactory.PromptYesNo("Discard Comment?", "Are you sure you want to discard this comment?");
            });
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
        }
    }
}

