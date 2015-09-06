namespace CodeFramework.Core.Utils
{
    public class RepositoryIdentifier
    {
        public string Owner { get; set; }
        public string Name { get; set; }

        public RepositoryIdentifier()
        {
        }

        public RepositoryIdentifier(string id)
        {
            var split = id.Split('/');
            Owner = split[0];
            Name = split[1];
        }
    }
}

