using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class BudgetsRepository : BaseRepository<Budget, Entities>, IDisposable
    {
        private int _companyId;
        public BudgetsRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<Budget> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId);
        }

        public override Budget GetEntity(int id, params string[] includes)
        {
            Budget budget = base.GetEntity(id, includes);
            return budget.CompanyId == _companyId ? budget : null;
        }

        public override bool Create(Budget entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}