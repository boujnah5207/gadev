using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;

namespace GAppsDev.Models
{
    public class UserPermissionsModel
    {
        public int UserId { get; set; }
        public SelectList PermissionsSelectList { get; set; }
        public List<UserPermission> UserPermissions { get; set; }
    }

    public class UserPermission
    {
        public bool IsActive { get; set; }
        public Budgets_Baskets Permission { get; set; }
    }
}