using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models
{
    public class ApprovalRouteModel
    {
        public Users_ApprovalRoutes ApprovalRoute { get; set; }
        public string Name { get; set; }

        public List<ApprovalLevelItem> Steps { get; set; }

        public ApprovalRouteModel()
        {
            Steps = new List<ApprovalLevelItem>();
        }
    }

    public class ApprovalLevelItem
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int StepNumber { get; set; }

    }
}