using System.Threading.Tasks;
using CodeHub.Core.ViewModels;

namespace CodeHub.Core.Services
{
    public interface INavigationService
    {
        Task NavigateTo(IBaseViewModel viewModel);

        Task Dismiss();
    }
}

