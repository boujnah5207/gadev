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
            entity.CreationDate = DateTime.Now;
            entity.WasAddedToInventory = false;
            return base.Create(entity);
        }
    }
}
