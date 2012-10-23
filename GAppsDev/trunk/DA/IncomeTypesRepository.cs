using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibraries;
using DB;

namespace DA
{
    public class IncomeTypesRepository : BaseRepository<Budgets_Incomes_types, Entities>, IDisposable
    {
        public override bool Create(Budgets_Incomes_types entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
