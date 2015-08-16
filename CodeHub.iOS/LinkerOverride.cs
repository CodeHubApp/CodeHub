using System;
using Akavache.Sqlite3;
using Foundation;

namespace CodeHub.iOS
{
    [Preserve]
    public static class LinkerPreserve
    {
        [Preserve]
        static LinkerPreserve()
        {
            string str = string.Empty;
            str = typeof(SQLitePersistentBlobCache).FullName;
            str = typeof(Splat.WrappingFullLogger).FullName;
            str = typeof(Splat.PlatformBitmapLoader).FullName;
            str = typeof(Splat.LogHost).FullName;
        }
    }

}
