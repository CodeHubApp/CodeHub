using System.Threading.Tasks;
using System.Net;
using System;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels
{
    public abstract class LoadableViewModel : BaseViewModel
    {
        public ReactiveCommand<Unit, Unit> LoadCommand { get; }

        private async Task LoadResource()
        {
            var retry = false;
            while (true)
            {
                if (retry)
                    await Task.Delay(100);

                try
                {
                    await Load();
                    return;
                }
                catch (WebException)
                {
                    if (!retry)
                        retry = true;
                    else
                        throw;
                }
            }
        }

        protected LoadableViewModel()
        {
            LoadCommand = ReactiveCommand.CreateFromTask(HandleLoadCommand);

            LoadCommand
                .ThrownExceptions
                .Select(err => new UserError("There was a problem loading the data from GitHub!", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();
        }

        private async Task HandleLoadCommand()
        {
            try
            {
                await LoadResource();
            }
            catch (OperationCanceledException e)
            {
                // The operation was canceled... Don't worry
                System.Diagnostics.Debug.WriteLine("The operation was canceled: " + e.Message);
            }
        }

        protected abstract Task Load();
    }
}

