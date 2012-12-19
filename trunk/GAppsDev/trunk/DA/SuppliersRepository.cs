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
        public enum Messeges
        {
            Error_ExternalIdExist,
            CreatedSuccessfully,
            CreationError,
        }

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

        public new Messeges Create(Supplier entity)
        {
            using (SuppliersRepository suppliersRepository = new SuppliersRepository(_companyId))
                if (suppliersRepository.GetList().Any(x => x.ExternalId == entity.ExternalId))
                    return Messeges.Error_ExternalIdExist;
            entity.CreationDate = DateTime.Now;
            entity.CompanyId = _companyId;
            if (!base.Create(entity)) return Messeges.CreationError;
            return Messeges.CreatedSuccessfully;
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
