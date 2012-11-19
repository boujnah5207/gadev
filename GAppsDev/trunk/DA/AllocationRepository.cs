using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class AllocationRepository : BaseRepository<Budgets_Allocations, Entities>, IDisposable
    {
        public override bool Create(Budgets_Allocations entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
