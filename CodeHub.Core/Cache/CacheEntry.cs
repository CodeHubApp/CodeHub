using SQLite;

namespace CodeFramework.Core.Cache
{
    public class CacheEntry
    {
        [AutoIncrement]
        [PrimaryKey]
        public int Id { get; set; }

        [Indexed]
        [MaxLength(1024)]
        public string Query { get; set; }

        [MaxLength(1024)]
        public string Path { get; set; }

        [MaxLength(256)]
        public string CacheTag { get; set; }

        [Ignore]
        public bool IsValid
        {
            get
            {
                return System.IO.File.Exists(Path);
            }
        }

        public bool Delete()
        {
            try
            {
                if (!IsValid) return false;
                System.IO.File.Delete(Path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public byte[] LoadResult()
        {
            try
            {
                return System.IO.File.ReadAllBytes(Path);
//				using (var io = System.IO.File.OpenRead(Path))
//				{
//					var s = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//					return (T)s.Deserialize(io);
//				}
            }
            catch (System.Exception e)
            {
                return null;
            }
        }

        public void SaveResult(byte[] data)
        {
            System.IO.File.WriteAllBytes(Path, data);
//			using (var io = System.IO.File.OpenWrite(Path))
//            {
//                var s = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//                s.Serialize(io, data);
//            }
        }
    }
}

