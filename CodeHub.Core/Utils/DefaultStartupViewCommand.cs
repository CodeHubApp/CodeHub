using System;

namespace CodeHub.Core.Utils
{
    public class PotentialStartupViewAttribute : Attribute
    {
        public string Name { get; private set; }

        public PotentialStartupViewAttribute(string name)
        {
            Name = name;
        }
    }

    public class DefaultStartupViewAttribute : Attribute
    {
    }
}

