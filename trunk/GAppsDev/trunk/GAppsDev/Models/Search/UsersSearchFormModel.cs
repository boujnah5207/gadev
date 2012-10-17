using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GAppsDev.Models.Search
{
    public class UsersSearchFormModel
    {
        public UsersSearchValuesModel SearchValues { get; set; }

        public SelectList RolesList { get; set; }
    }
}