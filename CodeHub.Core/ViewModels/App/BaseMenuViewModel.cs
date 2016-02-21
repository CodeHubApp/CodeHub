using System.Linq;
using System.Windows.Input;
using CodeHub.Core.Utils;
using System.Collections.Generic;
using CodeHub.Core.Services;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

namespace CodeHub.Core.ViewModels.App
{
    public abstract class BaseMenuViewModel : BaseViewModel
    {
        private static readonly IDictionary<string, string> Presentation = new Dictionary<string, string> {{PresentationValues.SlideoutRootPresentation, string.Empty}};  

        private static IAccountsService Accounts
        {
            get { return Mvx.Resolve<IAccountsService>(); }
        }

        public ICommand GoToDefaultTopView
        {
            get
            {
                var startupViewName = Accounts.ActiveAccount.DefaultStartupView;
                if (!string.IsNullOrEmpty(startupViewName))
                {
                    var props = from p in GetType().GetProperties()
                                let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true)
                                where attr.Length == 1
                                select new { Property = p, Attribute = attr[0] as PotentialStartupViewAttribute};

                    foreach (var p in props)
                    {
                        if (string.Equals(startupViewName, p.Attribute.Name))
                            return p.Property.GetValue(this) as ICommand;
                    }
                }

                //Oh no... Look for the last resort DefaultStartupViewAttribute
                var deprop = (from p in GetType().GetProperties()
                              let attr = p.GetCustomAttributes(typeof(DefaultStartupViewAttribute), true)
                              where attr.Length == 1
                              select new { Property = p, Attribute = attr[0] as DefaultStartupViewAttribute }).FirstOrDefault();

                //That shouldn't happen...
                if (deprop == null)
                    return null;
                var val = deprop.Property.GetValue(this);
                return val as ICommand;
            }
        }

        public ICommand DeletePinnedRepositoryCommand
        {
            get 
            {
                return new MvxCommand<CodeHub.Core.Data.PinnedRepository>(x => Accounts.ActiveAccount.PinnnedRepositories.RemovePinnedRepository(x.Id), x => x != null);
            }
        }

        protected bool ShowMenuViewModel<T>(object data) where T : IMvxViewModel
        {
            return this.ShowViewModel<T>(data, new MvxBundle(Presentation));
        }

        public IEnumerable<CodeHub.Core.Data.PinnedRepository> PinnedRepositories
        {
            get { return Accounts.ActiveAccount.PinnnedRepositories; }
        }
    }
}

