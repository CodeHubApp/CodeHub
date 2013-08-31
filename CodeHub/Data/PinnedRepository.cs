using SQLite;

namespace CodeHub.Data
{
    public class PinnedRepository
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public int AccountId { get; set; }

		public string Owner { get; set; }

		public string Slug { get; set; }

		public string Name { get; set; }

		public string ImageUri { get; set; }
    }
}

