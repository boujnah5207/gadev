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

        public OrdersHistoryRepository(int companyId, int userId)
        {
            _companyId = companyId;
            _userId = userId;
        }

        public bool Create(Orders_History entity, int actionId, string notes = null)
        {
            entity.CompayId = _companyId;
            entity.UserId = _userId;
            entity.OrderHistoryActionId = actionId;
            entity.Notes = notes;
            return base.Create(entity);
        }
    }
}
