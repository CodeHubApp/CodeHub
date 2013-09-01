using System;
using CodeFramework.Filters.Models;

namespace CodeHub.Filters.Models
{
    public class RepositoriesFilterModel : FilterModel<RepositoriesFilterModel>
    {
        public Order OrderBy { get; set; }
        public bool Ascending { get; set; }

        public RepositoriesFilterModel()
        {
            OrderBy = Order.Name;
            Ascending = true;
        }

        public override RepositoriesFilterModel Clone()
        {
            return (RepositoriesFilterModel)this.MemberwiseClone();
        }

        public enum Order : int
        { 
            Name, 
            Owner,
            [EnumDescription("Last Updated")]
            LastUpdated,
            Followers,
            Forks,
            [EnumDescription("Created Date")]
            CreatedOn, 
        };
    }
}

