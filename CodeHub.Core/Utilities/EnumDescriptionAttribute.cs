using System;

namespace CodeHub.Core.Utilities
{
    public class EnumDescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public EnumDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}

