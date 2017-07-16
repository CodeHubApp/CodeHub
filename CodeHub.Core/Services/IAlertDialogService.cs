using System;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using ReactiveUI;

namespace CodeHub.Core.Services
{
    public interface IAlertDialogService
    {
        Task<bool> PromptYesNo(string title, string message);

        Task Alert(string title, string message);

        Task<string> PromptTextBox(string title, string message, string defaultValue, string okTitle);

        void Show(string text);

        void Hide();
    }

    public static class AlertDialogServiceExtensions
    {
        public static IDisposable Activate(this IAlertDialogService @this, string text)
        {
            @this.Show(text);
            return Disposable.Create(@this.Hide);
        }

        public static IDisposable Activate(this IAlertDialogService @this, IObservable<bool> observable, string text)
        {
            return observable.Subscribe(x =>
            {
                if (x)
                    @this.Show(text);
                else
                    @this.Hide();
            });
        }

        public static IDisposable Activate(this IAlertDialogService @this, ReactiveCommand command, string text)
        {
            return command.IsExecuting.Subscribe(x =>
            {
                if (x)
                    @this.Show(text);
                else
                    @this.Hide();
            });
        }

        public static IDisposable AlertExecuting(this ReactiveCommand @this, IAlertDialogService dialogFactory, string text)
        {
            return @this.IsExecuting.Subscribe(x =>
            {
                if (x)
                    dialogFactory.Show(text);
                else
                    dialogFactory.Hide();
            });
        }
    }
}

