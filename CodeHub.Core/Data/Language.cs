using System.Diagnostics;

namespace CodeHub.Core.Data
{
    [DebuggerDisplay("{Name}")]
    public class Language
    {
        public Language() { }

        public Language(string name, string slug)
        {
            Name = name;
            Slug = slug;
        }

        public string Name { get; set; }

        public string Slug { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Language))
                return false;
            var other = (Language)obj;
            return Name == other.Name && Slug == other.Slug;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name != null ? Name.GetHashCode() : 0) ^ (Slug != null ? Slug.GetHashCode() : 0);
            }
        }
    }
}

