using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.Utilities;
using CodeHub.Core.Data;
using Xamarin.Utilities.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
	public abstract class BaseMenuViewModel : BaseViewModel
	{
	    protected readonly IAccountsService AccountsService;

		public ICommand GoToDefaultTopView
		{
			get
			{
                var startupViewName = AccountsService.ActiveAccount.DefaultStartupView;
				if (!string.IsNullOrEmpty(startupViewName))
				{
					var props = from p in GetType().GetRuntimeProperties()
	                            let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true).ToList()
	                            where attr.Count == 1
	                            select new { Property = p, Attribute = attr[0] as PotentialStartupViewAttribute};

					foreach (var p in props)
					{
						if (string.Equals(startupViewName, p.Attribute.Name))
							return p.Property.GetValue(this) as ICommand;
					}
				}

				//Oh no... Look for the last resort DefaultStartupViewAttribute
				var deprop = (from p in GetType().GetRuntimeProperties()
				              let attr = p.GetCustomAttributes(typeof(DefaultStartupViewAttribute), true).ToList()
				              where attr.Count == 1
							  select new { Property = p, Attribute = attr[0] as DefaultStartupViewAttribute }).FirstOrDefault();

				//That shouldn't happen...
				if (deprop == null)
					return null;
				var val = deprop.Property.GetValue(this);
				return val as ICommand;
			}
		}

		public IReactiveCommand<object> DeletePinnedRepositoryCommand { get; private set; }

        public IReactiveList<PinnedRepository> PinnedRepositories { get; private set; }

        protected BaseMenuViewModel(IAccountsService accountsService)
        {
            AccountsService = accountsService;
            DeletePinnedRepositoryCommand = ReactiveCommand.Create();
            PinnedRepositories = new ReactiveList<PinnedRepository>(AccountsService.ActiveAccount.PinnnedRepositories);

            DeletePinnedRepositoryCommand.OfType<PinnedRepository>()
                .Subscribe(x =>
                {
                    AccountsService.ActiveAccount.PinnnedRepositories.Remove(x);
                    AccountsService.Update(AccountsService.ActiveAccount);
                    PinnedRepositories.Remove(x);
                });
        }

        public void Init()
        {
            GoToDefaultTopView.Execute(null);
        }
    }
}

