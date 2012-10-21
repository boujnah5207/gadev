using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    class BudgetsExpensesToIncomesRepository : BaseRepository<Budgets_ExpensesToIncomes, Entities>, IDisposable
    {
        public override bool Create(Budgets_ExpensesToIncomes entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
