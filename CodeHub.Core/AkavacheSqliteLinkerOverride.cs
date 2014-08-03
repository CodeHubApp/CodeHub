using System;
using Akavache.Sqlite3;

// Note: This class file is *required* for iOS to work correctly, and is 
// also a good idea for Android if you enable "Link All Assemblies".
namespace WorkaroundAssemblyEarlyLoadingAndPreventLinkerStripping
{
    [Preserve]
    public static class LinkerPreserve
    {
        static LinkerPreserve()
        {
            Console.WriteLine(typeof(SQLitePersistentBlobCache));
        }
    }


    public class PreserveAttribute : Attribute
    {
    }
}
