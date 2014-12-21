using System;

namespace CodeHub.Core.Filters
{
    public class RepositoriesFilterModel
    {
        public Order OrderBy { get; set; }

        public bool Ascending { get; set; }

        public RepositoriesFilterModel()
        {
            OrderBy = Order.Name;
            Ascending = true;
        }

        public RepositoriesFilterModel Clone()
        {
            return (RepositoriesFilterModel)MemberwiseClone();
        }

        public enum Order
        { 
            Name, 
            Owner,
            //[EnumDescription("Last Updated")]
            LastUpdated,
            Followers,
            Forks,
            //[EnumDescription("Created Date")]
            CreatedOn, 
        };
    }
}

