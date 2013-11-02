using System;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.Services;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels
{
    /// <summary>
    ///    Defines the BaseViewModel type.
    /// </summary>
    public abstract class BaseViewModel : MvxViewModel
    {
        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>An instance of the service.</returns>
        public TService GetService<TService>() where TService : class
        {
            return Mvx.Resolve<TService>();
        }

        /// <summary>
        /// Gets the application for all view models
        /// </summary>
        protected IApplicationService Application
        {
            get { return GetService<IApplicationService>();  }
        }

        protected static void ReportError(Exception e)
        {
            Mvx.Resolve<IErrorReporter>().ReportError(e);
        }

        protected static void ReportError(string message, Exception e)
        {
            Mvx.Resolve<IErrorReporter>().ReportError(message, e);
        }
    }
}
