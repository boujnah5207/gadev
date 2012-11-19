using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class SuppliersRepository : BaseRepository<Supplier, Entities>
    {
        public override bool Create(Supplier entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
        public override bool AddList(List<Supplier> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].CreationDate = DateTime.Now;
            }
            return base.AddList(list);
        }
    }
}
