using SQLite;

namespace CodeHub.Data
{
    public class Filter
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public int AccountId { get; set; }

        [Unique]
        public string Type { get; set; }

        [MaxLengthAttribute(1024)]
        public string RawData { get; set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <returns>The data.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetData<T>() where T : new()
        {
            try
            {
                return RestSharp.SimpleJson.DeserializeObject<T>(RawData);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="o">O.</param>
        public void SetData(object o)
        {
            RawData = RestSharp.SimpleJson.SerializeObject(o);
        }
    }
}

