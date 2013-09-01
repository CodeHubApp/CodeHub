using System;
using CodeFramework.Filters.Models;

namespace CodeHub.Filters.Models
{
    public class SourceFilterModel : FilterModel<SourceFilterModel>
    {
        public Order OrderBy { get; set; }
        public bool Ascending { get; set; }

        public SourceFilterModel()
        {
            OrderBy = Order.FoldersThenFiles;
            Ascending = true;
        }

        public override SourceFilterModel Clone()
        {
            return (SourceFilterModel)this.MemberwiseClone();
        }

        public enum Order : int
        { 
            Alphabetical, 
            [EnumDescription("Folders Then Files")]
            FoldersThenFiles,
        };
    }
}

