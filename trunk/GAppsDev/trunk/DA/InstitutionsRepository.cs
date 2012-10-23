using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibraries;
using DB;

namespace DA
{
    public class InstitutionsRepository : BaseRepository<Budgets_Incomes_Institutions, Entities>, IDisposable
    {
        public override bool Create(Budgets_Incomes_Institutions entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
