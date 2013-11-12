using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels
{
    public abstract class BaseViewModel : CodeFramework.Core.ViewModels.BaseViewModel
    {
        public IApplicationService Application
        {
            get { return GetService<IApplicationService>(); }
        }
    }

    public abstract class LoadableViewModel : BaseViewModel, ILoadableViewModel
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

        protected LoadableViewModel()
        {
            _loadCommand = new MvxCommand<bool?>(async forceCacheInvalidation =>
            {
                try
                {
                    IsLoading = true;
                    await Load(forceCacheInvalidation ?? false);
                }
                catch (Exception)
                {
                    Console.WriteLine("I had trouble!!!");
                    throw;
                }
                finally
                {
                    IsLoading = false;
                }
            }, _ => IsLoading == false);
        }

        protected abstract Task Load(bool forceCacheInvalidation);
    }
}
