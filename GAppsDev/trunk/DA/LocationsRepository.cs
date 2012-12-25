using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class LocationsRepository : BaseRepository<Location, Entities>, IDisposable
    {
        private int _companyId;
        public LocationsRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<Location> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId);
        }

        public override Location GetEntity(int id, params string[] includes)
        {
            Location location = base.GetEntity(id, includes);
            return location != null && location.CompanyId == _companyId ? location : null;
        }

        public override bool Create(Location entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
