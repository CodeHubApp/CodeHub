namespace CodeHub.Core.Utils
{
    public class RepositoryIdentifier
    {
        public string Owner { get; }
        public string Name { get; }

        public RepositoryIdentifier(string owner, string name)
        {
            Owner = owner;
            Name = name;
        }

        public static RepositoryIdentifier FromFullName(string id)
        {
            var split = id.Split(new [] { '/' }, 2);
            return split.Length != 2 ? null : new RepositoryIdentifier(split[0], split[1]);
        }
    }
}

