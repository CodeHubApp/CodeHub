namespace CodeHub.Core.Data
{
    public class ImgurResponse
    {
        public ImgurDataModel Data { get; set; }

        public bool Success { get; set; }

        public class ImgurDataModel
        {
            public string Link { get; set; }
        }
    }
}

