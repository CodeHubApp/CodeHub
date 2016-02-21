using CodeHub.Core.ViewModels.App;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.App
{
    public class DefaultStartupViewModel : BaseDefaultStartupViewModel
    {
        public DefaultStartupViewModel(IAccountsService accountsService)
            : base(accountsService, typeof(MenuViewModel))
        {
        }
    }
}

