using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibraries;
using DB;

namespace DA
{
    public class ApprovalRoutesRepository : BaseRepository<Users_ApprovalRoutes, Entities>
    {
        private int _companyId;
        public ApprovalRoutesRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<Users_ApprovalRoutes> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId);
        }

        public override Users_ApprovalRoutes GetEntity(int id, params string[] includes)
        {
            Users_ApprovalRoutes route = base.GetEntity(id, includes);
            return route.CompanyId == _companyId ? route : null;
        }

        public override bool Create(Users_ApprovalRoutes entity)
        {
            entity.CompanyId = _companyId;
            return base.Create(entity);
        }
    }
}
