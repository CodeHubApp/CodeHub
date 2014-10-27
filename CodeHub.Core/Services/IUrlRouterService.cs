using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.Services
{
    public interface IUrlRouterService
    {
        IBaseViewModel Handle(string url);
    }
}

