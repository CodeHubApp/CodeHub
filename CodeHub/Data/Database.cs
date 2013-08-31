using SQLite;
using MonoTouch;

namespace CodeHub.Data
{
    public class Database : SQLiteConnection
    {
        private static readonly string DatabaseFilePath = System.IO.Path.Combine(Utilities.BaseDir, "Documents/data.db");
        private static Database _database;

        public static Database Main
        {
            get { return _database ?? (_database = new Database(DatabaseFilePath)); }
        }

        private Database(string file) 
			: base(file)
        {
            //Execute the typical stuff
			CreateTable<Account>();
            CreateTable<PinnedRepository>();
            CreateTable<Filter>();
        }
    }
}

