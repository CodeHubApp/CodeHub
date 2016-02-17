using System;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.Services
{
    public interface IViewModelService
    {
        T Create<T>() where T : class, IMvxViewModel;
    }

    public class ViewModelService : IViewModelService
    {
        private readonly IMvxViewModelLoader _loader;

        public ViewModelService(IMvxViewModelLoader loader)
        {
            _loader = loader;
        }

        public T Create<T>() where T : class, IMvxViewModel
        {
            return _loader.LoadViewModel(new MvxViewModelRequest(typeof(T), null, null, null), null) as T;
        }
    }
}

