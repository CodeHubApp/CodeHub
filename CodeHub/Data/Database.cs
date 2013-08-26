using System.IO;
using SQLite;
using MonoTouch;
using System.Linq;
using System.Collections.Generic;
using System;

namespace CodeHub.Data
{
    public class Database : SQLiteConnection
    {
        private static readonly string DatabaseFilePath = System.IO.Path.Combine(Utilities.BaseDir, "Documents/data.db");
        private static Database _database;

        public static Database Main
        {
            get 
            {
                if (_database == null)
                    _database = new Database(DatabaseFilePath);
                return _database;
            }
        }

        private Database(string file) 
			: base(file)
        {
            //Execute the typical stuff
			CreateTable<CodeHub.Data.Account>();
            CreateTable<CodeHub.Data.PinnedRepository>();
            CreateTable<CodeHub.Data.Filter>();
        }
    }
}

