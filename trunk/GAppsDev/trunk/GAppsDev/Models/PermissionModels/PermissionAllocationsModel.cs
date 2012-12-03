using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DB;

namespace GAppsDev.Models.PermissionModels
{
    public class PermissionAllocationsModel
    {
        public Budgets_Baskets Basket { get; set; }
        public BudgetAllocations BudgetAllocations { get; set; }
    }

    public class BudgetAllocations
    {
        public Budget Budget{ get; set; }
        public SelectList AllocationsList { get; set; }
        public List<PermissionAllocation> PermissionAllocations { get; set; }

        public BudgetAllocations()
        {
            PermissionAllocations = new List<PermissionAllocation>();
        }
    }

    public class PermissionAllocation
    {
        public bool IsActive { get; set; }
        public Budgets_BasketsToAllocation Allocation { get; set; }
    }
}