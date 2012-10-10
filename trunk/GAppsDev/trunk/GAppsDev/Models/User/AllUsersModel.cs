using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models.UserModels
{
    public class AllUsersModel
    {
        public List<User> ActiveUsers { get; set; }
        public List<PendingUser> PendingUsers { get; set; }
        public List<User> NonActiveUsers { get; set; }

        public int UsersCount { get { return ActiveUsers.Count + PendingUsers.Count; } }
        public int UsersLimit { get; set; }

        public AllUsersModel()
        {
            ActiveUsers = new List<User>();
            PendingUsers = new List<PendingUser>();
            NonActiveUsers = new List<User>();
        }
    }
}