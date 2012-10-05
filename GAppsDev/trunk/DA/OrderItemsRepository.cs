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
        public override bool Create(Orders_Items entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
