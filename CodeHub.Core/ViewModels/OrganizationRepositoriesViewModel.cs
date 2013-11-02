using System;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class OrganizationRepositoriesViewModel : RepositoriesViewModel
    {
        public string Name
        {
            get;
            private set;
        }

        public OrganizationRepositoriesViewModel(string name)
            : base(name)
        {
            Name = name;
        }

        public override Task Load(bool forceDataRefresh)
        {
            return Repositories.SimpleCollectionLoad(Application.Client.Organizations[Name].Repositories.GetAll(), forceDataRefresh);
        }
    }
}

