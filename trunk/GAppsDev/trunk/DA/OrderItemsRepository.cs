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
        private int _supplierId;

        public OrderItemsRepository(int companyId, int supplierId)
        {
            _companyId = companyId;
            _supplierId = supplierId;
        }

        public override IQueryable<Orders_Items> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId && x.SupplierId == _supplierId);
        }

        public override Orders_Items GetEntity(int id, params string[] includes)
        {
            Orders_Items Items = base.GetEntity(id, includes);
            return Items.CompanyId == _companyId ? Items : null;
        }

        public override bool Create(Orders_Items entity)
        {
            Orders_Items item = GetExist(entity);
            if (item != null)
            {
                item.IsCanceled = false;
                this.Update(item);
                entity = item;
                return true;
            }

            entity.SupplierId = _supplierId;
            entity.CompanyId = _companyId;
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }

        public Orders_Items GetExist(Orders_Items orderitem)
        {
                return (this.GetList().SingleOrDefault(x => x.Title == orderitem.Title && x.SubTitle == orderitem.SubTitle));
        }
    }
}
