using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    class BudgetsIncomesRepository : BaseRepository<Budgets_Incomes, Entities>, IDisposable
    {
        public override bool Create(Budgets_Incomes entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
