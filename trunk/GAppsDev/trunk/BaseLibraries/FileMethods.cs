using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BaseLibraries
{
    public static class FileMethods
    {
        public static byte[] ExportCsv<T>(string path, IEnumerable<T> entities)
        {
            IEnumerable<PropertyInfo> entityProperties = typeof(T).GetProperties();

            entityProperties = entityProperties.Where(x => x.GetIndexParameters().Length == 0);
            StringBuilder csv = new StringBuilder();

            IEnumerable<string> titels = entityProperties.Select(x => x.Name);
            csv.AppendLine(string.Join(",", titels));

            foreach (var item in entities)
            {
                IEnumerable<string> content = entityProperties.Select(x => (x.GetValue(item, null) ?? string.Empty).ToString());
                csv.AppendLine(string.Join(",", content));
                
            }

            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }
    }
}
