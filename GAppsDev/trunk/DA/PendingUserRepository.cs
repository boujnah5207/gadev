using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class PendingUserRepository : BaseRepository<PendingUser, Entities>, IDisposable
    {
        public override bool Create(PendingUser entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
