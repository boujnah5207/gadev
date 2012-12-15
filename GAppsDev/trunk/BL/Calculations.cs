using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DA;
using DB;

namespace BL
{
    public static class Calculations
    {
        public static decimal RemainingAllocationAmount(Budgets_Allocations allocation, int endMonth = 12, int startMonth = 1)
        {
            List<Orders_OrderToAllocation> approvedAllocations = 
                allocation
                .Orders_OrderToAllocation
                .Where(x => x.Order.StatusId != (int)StatusType.Declined && x.Order.StatusId != (int)StatusType.OrderCancelled)
                .ToList();

            List<Budgets_AllocationToMonth> allocationMonths = 
                allocation
                .Budgets_AllocationToMonth
                .Where(x => x.MonthId >= startMonth && x.MonthId <= endMonth)
                .ToList();

            decimal? totalAmount = allocationMonths.Sum( x => (int?)x.Amount);
            decimal? usedAmount = approvedAllocations.Where(x => x.MonthId >= startMonth && x.MonthId <= endMonth).Select(d => (decimal?)d.Amount).Sum();

            return (totalAmount.HasValue ? totalAmount.Value : 0) - (usedAmount.HasValue ? usedAmount.Value : 0);
        }

        
    }
}
