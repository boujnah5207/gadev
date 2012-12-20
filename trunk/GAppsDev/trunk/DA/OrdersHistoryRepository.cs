using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class OrdersHistoryRepository : BaseRepository<Orders_History, Entities>, IDisposable
    {
        private int _companyId;
        private int _userId;
        private int _orderId;

        public OrdersHistoryRepository(int companyId, int userId, int orderId)
        {
            _companyId = companyId;
            _userId = userId;
            _orderId = orderId;
        }

        public bool Create(Orders_History entity, int actionId, string notes = null)
        {
            entity.CompayId = _companyId;
            entity.UserId = _userId;
            entity.OrderId = _orderId;
            entity.OrderHistoryActionId = actionId;
            entity.Notes = notes;
            return base.Create(entity);
        }
    }
}
