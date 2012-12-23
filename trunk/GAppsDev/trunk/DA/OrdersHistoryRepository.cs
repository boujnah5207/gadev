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
        public override IQueryable<Orders_History> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId && x.OrderId == _orderId);
        }
        public bool Create(Orders_History entity, int actionId, string notes = null)
        {
            entity.CreationDate = DateTime.Now;
            entity.CompanyId = _companyId;
            entity.UserId = _userId;
            entity.OrderId = _orderId;
            entity.OrderHistoryActionId = actionId;
            entity.Notes = notes;
            return base.Create(entity);
        }
    }
}
