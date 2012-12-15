using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class OrdersRepository : BaseRepository<Order, Entities>
    {
        private int _companyId;
        public OrdersRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<Order> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId);
        }

        public override Order GetEntity(int id, params string[] includes)
        {
            Order order = base.GetEntity(id, includes);
            return order.CompanyId == _companyId ? order : null;
        }

        public override bool Create(Order entity)
        {
            int? latestOrderNumber;
            latestOrderNumber = this.GetList().Select(x => (int?)x.OrderNumber).Max();
            if (latestOrderNumber.HasValue)
                latestOrderNumber++;
            else
                latestOrderNumber = 1;

            entity.OrderNumber = latestOrderNumber.Value;
            entity.CreationDate = DateTime.Now;
            entity.StatusId = (int)StatusType.Pending;
            entity.LastStatusChangeDate = DateTime.Now;
            entity.WasAddedToInventory = false;
            entity.CompanyId = _companyId;

            return base.Create(entity);
        }

        public ExeedingOrderData GetOrderWithExeedingData(int id, params string[] includes)
        {
            ExeedingOrderData data = new ExeedingOrderData();
            data.ExeedingMonthAllocations = new List<ExeedingAllocationMonthData>();

            data.OriginalOrder = this.GetEntity(id, includes);
            if (data.OriginalOrder == null) return null;

            var orderAllocationIds = data.OriginalOrder.Orders_OrderToAllocation.Select(x => x.AllocationId).Distinct();

            using (AllocationRepository allocationsRep = new AllocationRepository(_companyId))
            {
                foreach (var allocationId in orderAllocationIds)
                {
                    var allocationData = allocationsRep.GetAllocationsData(new int[] { allocationId }, StatusType.Pending).First();

                    var previousOrdersMonthAllocations =
                        allocationData
                        .OriginalAllocation
                        .Orders_OrderToAllocation
                        .Where(x =>
                                x.Order.StatusId != (int)StatusType.Declined &&
                                x.Order.StatusId != (int)StatusType.OrderCancelled &&
                                x.Order.CreationDate < data.OriginalOrder.CreationDate
                               )
                        .ToList();

                    var orderMonthAllocations =
                        data.OriginalOrder
                        .Orders_OrderToAllocation
                        .Where(x => x.AllocationId == allocationData.AllocationId)
                        .ToList();

                    foreach (var month in orderMonthAllocations)
                    {
                        var monthData = allocationData.Months.Single( x => x.MonthId == month.MonthId);
                        decimal orderAmountFromMonth = orderMonthAllocations.Where( x => x.MonthId == month.MonthId).Sum( x => x.Amount);
                        decimal previousAmountFromMonth = previousOrdersMonthAllocations.Where( x => x.MonthId == month.MonthId).Sum( x => x.Amount);

                        if ((previousAmountFromMonth + orderAmountFromMonth) > monthData.TotalAmount)
                        {
                            var newData = new ExeedingAllocationMonthData()
                            {
                                AllocationId = allocationData.AllocationId,
                                MonthId = monthData.MonthId,
                                MonthAmount = monthData.TotalAmount,
                                AmountUsedByPreviousOrders = previousAmountFromMonth,
                                OrderAmountFromMonth = orderAmountFromMonth,
                                RemainingMonthAmount = monthData.RemainingAmount,
                                ExeedingAmount = (monthData.TotalAmount - (previousAmountFromMonth + orderAmountFromMonth)) * -1,
                            };

                            data.ExeedingMonthAllocations.Add(newData);
                        }
                    }
                }
            }

            return data;
        }

        public class ExeedingOrderData
        {
            public Order OriginalOrder { get; set; }
            public int OrderId { get; set; }

            public List<ExeedingAllocationMonthData> ExeedingMonthAllocations { get; set; }
        }

        public class ExeedingAllocationMonthData
        {
            public int AllocationId { get; set; }
            public int MonthId { get; set; }
            public decimal MonthAmount { get; set; }
            public decimal RemainingMonthAmount { get; set; }
            public decimal AmountUsedByPreviousOrders { get; set; }
            public decimal OrderAmountFromMonth { get; set; }
            public decimal ExeedingAmount { get; set; }
        }
    }
}
