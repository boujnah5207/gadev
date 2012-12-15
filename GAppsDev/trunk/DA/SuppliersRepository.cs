using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class SuppliersRepository : BaseRepository<Supplier, Entities>
    {
        private int _companyId;
        public SuppliersRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<Supplier> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId);
        }

        public override Supplier GetEntity(int id, params string[] includes)
        {
            Supplier supplier = base.GetEntity(id, includes);
            return supplier.CompanyId == _companyId ? supplier : null;
        }

        public override bool Create(Supplier entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }

        public override bool AddList(List<Supplier> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].CreationDate = DateTime.Now;
            }
            return base.AddList(list);
        }
    }
}
