using GitHubSharp;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using System.Windows.Input;
using System.Net;
using System;

namespace CodeHub.Core.ViewModels
{
    public abstract class LoadableViewModel : BaseViewModel
    {
        private readonly ICommand _loadCommand;
        private bool _isLoading;

        public ICommand LoadCommand
        {
            get { return _loadCommand; }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; RaisePropertyChanged(() => IsLoading); }
        }

        private async Task LoadResource(bool forceCacheInvalidation)
        {
            var retry = false;
            while (true)
            {
                if (retry)
                    await Task.Delay(100);

                try
                {
                    await Load(forceCacheInvalidation);
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

        protected async Task ExecuteLoadResource(bool forceCacheInvalidation)
        {
            try
            {
                await LoadResource(forceCacheInvalidation);
            }
            catch (System.IO.IOException)
            {
                DisplayAlert("Unable to communicate with GitHub as the transmission was interrupted! Please try again.");
            }
            catch (StatusCodeException e)
            {
                DisplayAlert(e.Message);

                if (e is NotFoundException)
                    ChangePresentation(new MvxClosePresentationHint(this));
            }
        }

        protected LoadableViewModel()
        {
            _loadCommand = new MvxCommand<bool?>(x => HandleLoadCommand(x), _ => !IsLoading);
        }

        private async Task HandleLoadCommand(bool? forceCacheInvalidation)
        {
            try
            {
                IsLoading = true;
                await ExecuteLoadResource(forceCacheInvalidation ?? false);
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

        protected abstract Task Load(bool forceCacheInvalidation);
    }
}

