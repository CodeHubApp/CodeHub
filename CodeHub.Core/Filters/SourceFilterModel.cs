using System;

namespace CodeHub.Core.Filters
{
	public class SourceFilterModel
    {
        public Order OrderBy { get; set; }

        public bool Ascending { get; set; }

		public SourceFilterModel()
        {
            OrderBy = Order.FoldersThenFiles;
            Ascending = true;
        }

		public SourceFilterModel Clone()
        {
			return (SourceFilterModel)MemberwiseClone();
        }

        public enum Order
        { 
            Alphabetical, 
            //[EnumDescription("Folders Then Files")]
            FoldersThenFiles,
        };
    }
}

