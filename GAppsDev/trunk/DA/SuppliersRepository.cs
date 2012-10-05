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
    }
}
