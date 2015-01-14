using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels
{
    public interface IBaseViewModel : ISupportsActivation, IProvidesTitle, IRoutingViewModel
    {
    }
}

