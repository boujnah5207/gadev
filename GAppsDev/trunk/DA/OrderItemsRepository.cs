using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class OrderItemsRepository : BaseRepository<Orders_Items, Entities>
    {
        private int _companyId;
        public OrderItemsRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<Orders_Items> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId);
        }

        public override Orders_Items GetEntity(int id, params string[] includes)
        {
            Orders_Items Items = base.GetEntity(id, includes);
            return Items.CompanyId == _companyId ? Items : null;
        }

        public override bool Create(Orders_Items entity)
        {
            entity.CompanyId = _companyId;
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
