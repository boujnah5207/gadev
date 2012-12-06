using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models.AllocationModels
{
    public class AllocationDetailsModel
    {
        public Budgets_Allocations OriginalAllocation { get; set; }
        public Budgets_Allocations RemainingAllocation { get; set; }
    }
}