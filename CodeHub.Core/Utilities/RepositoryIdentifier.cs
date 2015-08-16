namespace CodeHub.Core.Utilities
{
    public class RepositoryIdentifier
    {
        public string Owner { get; }
        public string Name { get; }

        public RepositoryIdentifier()
        {
        }

        public RepositoryIdentifier(string id)
        {
            var split = id.Split('/');
            Owner = split[0];
            Name = split[1];
        }

        public RepositoryIdentifier(string owner, string name)
        {
            Owner = owner;
            Name = name;
        }
    }
}

