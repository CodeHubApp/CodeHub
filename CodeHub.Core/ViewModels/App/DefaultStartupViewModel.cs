using CodeFramework.Core.Services;

namespace CodeHub.Core.ViewModels.App
{
	public class DefaultStartupViewModel : CodeFramework.Core.ViewModels.Application.DefaultStartupViewModel
    {
		public DefaultStartupViewModel(IAccountsService accountsService)
			: base(accountsService, typeof(MenuViewModel))
		{
		}
    }
}

