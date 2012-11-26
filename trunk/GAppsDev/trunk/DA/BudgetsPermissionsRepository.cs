using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class BudgetsPermissionsRepository : BaseRepository<Budgets_Baskets, Entities>, IDisposable
    {
        public override bool Create(Budgets_Baskets entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}



