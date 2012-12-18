﻿using System;
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
            return user.CompanyId == _companyId && !user.IsForManagment ? user : null;
        }
    }
}
