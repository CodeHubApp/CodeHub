using System;

namespace CodeHub.Core.Services
{
    public interface IViewModelViewService
    {
        void Register(Type viewModelType, Type viewType);

        Type GetViewFor(Type viewModel);

        void RegisterViewModels(System.Reflection.Assembly asm);
    }
}

