using System;

namespace CodeHub.Core.Models
{
    public class ImgurModel
    {
        public ImgurDataModel Data { get; set; }

        public bool Success { get; set; }

        public class ImgurDataModel
        {
            public string Link { get; set; }
        }
    }
}

