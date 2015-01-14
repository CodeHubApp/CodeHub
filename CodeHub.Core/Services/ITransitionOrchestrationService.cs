using ReactiveUI;

namespace CodeHub.Core.Services
{
    public interface ITransitionOrchestrationService
    {
        void Transition(IViewFor fromView, IViewFor toView);
    }
}

