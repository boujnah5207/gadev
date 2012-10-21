using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    class BudgetsRepository : BaseRepository<Budget, Entities>, IDisposable
    {
        public override bool Create(Budget entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}