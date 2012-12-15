using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;

namespace GAppsDev.Models.FileModels
{
    public class UploadedFile
    {
        public string FullPath { get; set; }
        public string Extension { get; set; }
        public string MimeType { get; set; }

        public byte[] Content { get; set; }

        public UploadedFile()
        {
            MimeType = MediaTypeNames.Application.Octet;
        }
    }
}