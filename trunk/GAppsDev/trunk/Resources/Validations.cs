using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Resources
{
    public static class Validations
    {
        public const long MAX_FILE_SIZE = 256000;

        public static string UploadedFile(HttpPostedFileBase file, long? maxBytes = MAX_FILE_SIZE)
        {
            if(file == null) return Loc.Dic.validation_IsNotFile;
            if(file.ContentLength == 0) return Loc.Dic.validation_FileEmpty;
            if (maxBytes.HasValue && file.ContentLength > maxBytes) return Loc.Dic.validation_FileTooBig;

            List<string> acceptedFileTypes = new List<string>()
            {
                "application/pdf",
                "image/jpeg",
                "image/png",
                "image/bmp",
                "image/tiff",
                "application/msword",
                "application/x-compressed",
                "application/x-rar-compressed",
                "application/x-gzip"
            };

            if (!acceptedFileTypes.Contains(file.ContentType)) return Loc.Dic.validation_InvalidFileType;

            return null;
        }
    }
}
