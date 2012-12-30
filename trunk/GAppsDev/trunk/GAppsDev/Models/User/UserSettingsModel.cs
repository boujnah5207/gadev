using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models
{
    public class UserSettingsModel
    {
        public int LanguageId { get; set; }

        [LocalizedName("SecondaryNotificationsEmail")]
        [LocalizedEmailAttribute]
        public string NotificationsEmail { get; set; }
    }
}