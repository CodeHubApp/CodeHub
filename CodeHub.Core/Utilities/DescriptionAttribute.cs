using System;

namespace CodeHub.Core.Utilities
{
    public class DescriptionAttribute : Attribute
    { 
        public string Description { get; }

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}

