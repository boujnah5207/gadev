using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    class BudgetsExpensesRepository : BaseRepository<Budgets_Expenses, Entities>, IDisposable
    {
        public override bool Create(Budgets_Expenses entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
