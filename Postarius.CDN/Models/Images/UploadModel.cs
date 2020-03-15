using System;
using System.Collections.Generic;

namespace Web.StaticFiles.Models.Images
{
    public class UploadModel
    {
        public int Count => Images.Count;
        public DateTime Date { get; set; }

        public IList<ImageInfo> Images { get; private set; }

        public UploadModel()
        {
            Images = new List<ImageInfo>();
        }

        public class ImageInfo
        {
            public string RawUrl { get; set; }
            public string Cropped150x150Url { get; set; }
            public string Cropped300x400Url { get; set; }
        }
    }
}