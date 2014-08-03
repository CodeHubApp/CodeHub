namespace CodeHub.Core.Data
{
    public class PinnedRepository
    {
		public string Owner { get; set; }

		public string Slug { get; set; }

		public string Name { get; set; }

		public string ImageUri { get; set; }

        protected bool Equals(PinnedRepository other)
        {
            return string.Equals(Owner, other.Owner) && string.Equals(Slug, other.Slug);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PinnedRepository) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Owner != null ? Owner.GetHashCode() : 0)*397) ^ (Slug != null ? Slug.GetHashCode() : 0);
            }
        }
    }
}

