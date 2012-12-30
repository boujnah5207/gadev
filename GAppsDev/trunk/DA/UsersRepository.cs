using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class UsersRepository : BaseRepository<User, Entities>
    {
        private int _companyId;
        public UsersRepository(int companyId)
        {
            _companyId = companyId;
        }

        public override IQueryable<User> GetList(params string[] includes)
        {
            return base.GetList(includes)
                .Where(x => x.CompanyId == _companyId && !x.IsForManagment);
        }

        public override User GetEntity(int id, params string[] includes)
        {
            User user = base.GetEntity(id, includes);
            return user != null && user.CompanyId == _companyId && !user.IsForManagment ? user : null;
        }

        public override bool Create(User entity)
        {
            entity.CreationTime = DateTime.Now;
            entity.LastLogInTime = DateTime.Now;
            entity.CompanyId = _companyId;
            entity.IsActive = true;
            entity.IsForManagment = false;
            entity.NotificationCode = Guid.NewGuid().ToString("N");

            return base.Create(entity);
        }
    }
}
