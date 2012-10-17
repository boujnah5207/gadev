using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models.Search
{
    public class UsersSearchValuesModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Role { get; set; }
        public string Email { get; set; }
        public DateTime? CreationMin { get; set; }
        public DateTime? CreationMax { get; set; }
    }
}