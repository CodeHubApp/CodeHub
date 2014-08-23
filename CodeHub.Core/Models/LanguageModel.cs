namespace CodeHub.Core.Models
{
    public class LanguageModel
    {
        public string Name { get; set; }

        public string Slug { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(LanguageModel))
                return false;
            var other = (LanguageModel)obj;
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

