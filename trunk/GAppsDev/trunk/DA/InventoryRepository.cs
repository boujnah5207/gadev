using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class InventoryRepository : BaseRepository<Inventory, Entities>, IDisposable
    {
        private int _companyId;
        public InventoryRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<Inventory> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId);
        }

        public override Inventory GetEntity(int id, params string[] includes)
        {
            Inventory inventory = base.GetEntity(id, includes);
            return inventory.CompanyId == _companyId ? inventory : null;
        }

        public override bool Create(Inventory entity)
        {
            int? latestInventarNumber;
            using (InventoryRepository inventoryRepository = new InventoryRepository(_companyId))
            {
                latestInventarNumber = inventoryRepository.GetList().Select(x => x.InventarNumber).Max();
                if (latestInventarNumber.HasValue)
                    latestInventarNumber++;
                else
                    latestInventarNumber = 1;
            }
            entity.InventarNumber = latestInventarNumber.Value;
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
