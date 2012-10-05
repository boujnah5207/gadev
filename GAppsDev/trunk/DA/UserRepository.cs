using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class UserRepository : BaseRepository<User, Entities>, IDisposable
    {
        public override bool Create(User entity)
        {
            entity.CreationTime = DateTime.Now;
            return base.Create(entity);
        }
    }
}
