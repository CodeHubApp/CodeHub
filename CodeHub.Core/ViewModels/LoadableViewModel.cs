using GitHubSharp;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using System.Windows.Input;
using System.Net;
using System;

namespace CodeHub.Core.ViewModels
{
    public abstract class LoadableViewModel : BaseViewModel
    {
        public ICommand LoadCommand { get; }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            private set { this.RaiseAndSetIfChanged(ref _isLoading, value); }
        }

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

        protected async Task ExecuteLoadResource()
        {
            try
            {
                await LoadResource();
            }
            catch (System.IO.IOException)
            {
                DisplayAlert("Unable to communicate with GitHub as the transmission was interrupted! Please try again.");
            }
            catch (StatusCodeException e)
            {
                DisplayAlert(e.Message);
            }
        }

        protected LoadableViewModel()
        {
            LoadCommand = new MvxCommand<bool?>(x => HandleLoadCommand(), _ => !IsLoading);
        }

        private async Task HandleLoadCommand()
        {
            try
            {
                IsLoading = true;
                await ExecuteLoadResource();
            }
            catch (OperationCanceledException e)
            {
                // The operation was canceled... Don't worry
                System.Diagnostics.Debug.WriteLine("The operation was canceled: " + e.Message);
            }
            catch (Exception e)
            {
                DisplayAlert("The request to load this item did not complete successfuly! " + e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected abstract Task Load();
    }
}

