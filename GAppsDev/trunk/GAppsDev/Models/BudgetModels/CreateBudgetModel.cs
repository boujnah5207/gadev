using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DB;

namespace GAppsDev.Models.BudgetModels
{
    public class CreateBudgetModel
    {
        public List<Budgets_Incomes> Incomes { get; set; }
        public List<Budgets_Expenses> Expenses { get; set; }

        public List<Budgets_Allocations> Allocations { get; set; }
        public List<Budgets_PermissionsToAllocation> PermissionsAllocations { get; set; }
    }
}