using System;
using CodeHub.Core.Services;
using System.Windows.Input;
using CodeHub.Core.Messages;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using MvvmCross.Platform;

namespace CodeHub.Core.ViewModels
{
    /// <summary>
    ///    Defines the BaseViewModel type.
    /// </summary>
    public abstract class BaseViewModel : MvxViewModel
    {
        /// <summary>
        /// Gets the go to URL command.
        /// </summary>
        /// <value>The go to URL command.</value>
        public ICommand GoToUrlCommand
        {
            get { return new MvxCommand<string>(x => ShowViewModel<WebBrowserViewModel>(new WebBrowserViewModel.NavObject { Url = x })); }
        }

        /// <summary>
        /// Gets the share command.
        /// </summary>
        /// <value>The share command.</value>
        public ICommand ShareCommand
        {
            get { return new MvxCommand<string>(x => GetService<IShareService>().ShareUrl(x), x => !string.IsNullOrEmpty(x)); }
        }

        /// <summary>
        /// Gets the ViewModelTxService
        /// </summary>
        /// <value>The tx sevice.</value>
        protected IViewModelTxService TxSevice
        {
            get { return GetService<IViewModelTxService>(); }
        }

        /// <summary>
        /// Gets the messenger service
        /// </summary>
        /// <value>The messenger.</value>
        protected IMvxMessenger Messenger
        {
            get { return GetService<IMvxMessenger>(); }
        }

        /// <summary>
        /// Gets the alert service
        /// </summary>
        /// <value>The alert service.</value>
        protected IAlertDialogService AlertService
        {
            get { return GetService<IAlertDialogService>(); }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>An instance of the service.</returns>
        protected TService GetService<TService>() where TService : class
        {
            return Mvx.Resolve<TService>();
        }

        /// <summary>
        /// Display an error message to the user
        /// </summary>
        /// <param name="message">Message.</param>
        protected void DisplayAlert(string message)
        {
            AlertService.Alert("Error!", message);
        }

        /// <summary>
        /// Reports the error by displaying it and reporting it to analytics
        /// </summary>
        /// <param name="e">E.</param>
        protected void ReportException(Exception e)
        {
            Messenger.Publish(new ErrorMessage(this, e));
        }
    }
}
