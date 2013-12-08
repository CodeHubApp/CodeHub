using CodeFramework.Core.ViewModels.App;
using CodeFramework.Core.Services;

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

