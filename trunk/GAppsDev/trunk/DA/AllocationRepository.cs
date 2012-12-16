using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class AllocationRepository : BaseRepository<Budgets_Allocations, Entities>, IDisposable
    {
        private int _companyId;
        public AllocationRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<Budgets_Allocations> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId && !x.IsCanceled);
        }

        public IQueryable<Budgets_Allocations> GetWithCanceled(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId);
        }

        public override Budgets_Allocations GetEntity(int id, params string[] includes)
        {
            Budgets_Allocations allocation = base.GetEntity(id, includes);
            return allocation.CompanyId == _companyId ? allocation : null;
        }

        public override bool Create(Budgets_Allocations entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }

        public List<AllocationAmountData> GetAllocationsData(int[] allocationIds, StatusType minOrderStatus = StatusType.ApprovedPendingInvoice, int? excludeOrder = null, int startMonth = 1, int endMonth = 12, params string[] includes)
        {
            List<AllocationAmountData> data = new List<AllocationAmountData>();

            string[] baseIncludes = { "Budgets_AllocationToMonth" };
            includes = baseIncludes.Union(includes).ToArray();

            List<Budgets_Allocations> allocations;

            allocations = this.GetWithCanceled(includes).Where(x => allocationIds.Contains(x.Id)).ToList();

            foreach (var allocation in allocations)
            {
                IEnumerable<Orders_OrderToAllocation> approvedAllocationsQuery =
                allocation
                .Orders_OrderToAllocation
                .Where(x => x.Order.StatusId >= (int)minOrderStatus && x.Order.StatusId != (int)StatusType.Declined && x.Order.StatusId != (int)StatusType.OrderCancelled);

                if (excludeOrder.HasValue)
                {
                    approvedAllocationsQuery = approvedAllocationsQuery.Where(x => x.OrderId != excludeOrder.Value);
                }

                List<Orders_OrderToAllocation> approvedAllocations = approvedAllocationsQuery.ToList();

                List<AllocationMonthAmountData> allocationMonthData = new List<AllocationMonthAmountData>();
                for (int monthId = startMonth; monthId <= endMonth; monthId++)
                {
                    var allocationMonth = allocation.Budgets_AllocationToMonth.SingleOrDefault(x => x.MonthId == monthId);

                    AllocationMonthAmountData newMonthData = new AllocationMonthAmountData()
                    {
                        OriginalAllocationMonth = allocationMonth,
                        MonthId = allocationMonth.MonthId,
                        TotalAmount = allocationMonth.Amount,
                        UsedAmount = approvedAllocations.Where(m => m.MonthId == monthId).Select(d => (decimal?)d.Amount).Sum() ?? 0
                    };

                    allocationMonthData.Add(newMonthData);
                }

                AllocationAmountData newData = new AllocationAmountData()
                {
                    OriginalAllocation = allocation,
                    AllocationId = allocation.Id,
                    TotalAmount = allocation.Budgets_AllocationToMonth.Select(x => (decimal?)x.Amount).Sum() ?? 0,
                    UsedAmount = approvedAllocations.Select(x => (decimal?)x.Amount).Sum() ?? 0,
                    Months = allocationMonthData
                };

                data.Add(newData);
            }

            return data;
        }

        public List<Orders_OrderToAllocation> GenerateOrderAllocations(AllocationAmountData data, decimal amount, bool allowExeeding = true, int maxMonth = 12, int? orderId = null)
        {
            List<Orders_OrderToAllocation> generatedAllocations = new List<Orders_OrderToAllocation>();

            decimal remainingAmountToAllocate = amount;
            for (int monthId = 1; monthId <= maxMonth && remainingAmountToAllocate > 0; monthId++)
            {
                var monthData = data.Months.Single(x => x.MonthId == monthId);

                decimal newAmount;
                if (!allowExeeding || monthId != maxMonth)
                {
                    if (monthData.RemainingAmount <= 0) continue;
                    newAmount = monthData.RemainingAmount < remainingAmountToAllocate ? monthData.RemainingAmount : remainingAmountToAllocate;
                }
                else
                {
                    newAmount = remainingAmountToAllocate;
                }

                Orders_OrderToAllocation newAllocation = new Orders_OrderToAllocation()
                {
                    AllocationId = data.AllocationId,
                    MonthId = monthData.MonthId,
                    Amount = newAmount,
                    OrderId = orderId ?? 0
                };

                remainingAmountToAllocate -= newAllocation.Amount;
                generatedAllocations.Add(newAllocation);
            }

            if (remainingAmountToAllocate > 0) return null;

            return generatedAllocations;
        }

        public List<Budgets_Allocations> GetUserAllocations(int userId, int budgetId, int? excludeOrder = null)
        {
            List<int> userAllocationIds = new List<int>();
            List<Budgets_Allocations> userAllocations = new List<Budgets_Allocations>();

            using (UsersToBasketsRepository usersToBasketsRep = new UsersToBasketsRepository())
            using (BasketsToAllocationsRepository basketsToAllocationRep = new BasketsToAllocationsRepository())
            {
                List<Budgets_UsersToBaskets> permissions =
                    usersToBasketsRep
                    .GetList()
                    .Where(x => x.UserId == userId)
                    .ToList();

                foreach (var permission in permissions)
                {
                    userAllocationIds.AddRange(
                        basketsToAllocationRep.GetList()
                            .Where(x => x.BasketId == permission.Budgets_Baskets.Id && x.BudgetId == budgetId)
                            .Select(x => x.Budgets_Allocations.Id)
                            .ToList()
                            );
                }

                userAllocationIds = userAllocationIds.Distinct().ToList();
            }

            var data = this.GetAllocationsData(userAllocationIds.ToArray(), StatusType.Pending, excludeOrder);

            foreach (var allocationData in data)
            {
                foreach (var monthData in allocationData.Months)
                {
                    monthData.OriginalAllocationMonth.Amount = monthData.RemainingAmount;
                }
            }

            return data.Select( x => x.OriginalAllocation).ToList();
        }

        public class AllocationAmountData
        {
            public Budgets_Allocations OriginalAllocation { get; set; }
            public int AllocationId { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal UsedAmount { get; set; }
            public List<AllocationMonthAmountData> Months { get; set; }

            public decimal RemainingAmount
            {
                get { return TotalAmount - UsedAmount; }
            }
            public decimal MonthsTotalAmount
            {
                get { return Months.Select(x => (decimal?)x.TotalAmount).Sum() ?? 0; }
            }
            public decimal MonthsUsedAmount
            {
                get { return Months.Select(x => (decimal?)x.UsedAmount).Sum() ?? 0; }
            }
            public decimal MonthsRemainingAmount
            {
                get { return MonthsTotalAmount - MonthsUsedAmount; }
            }
        }

        public class AllocationMonthAmountData
        {
            public Budgets_AllocationToMonth OriginalAllocationMonth { get; set; }
            public int MonthId { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal UsedAmount { get; set; }

            public decimal RemainingAmount
            {
                get { return TotalAmount - UsedAmount; }
            }
        }
    }
}
