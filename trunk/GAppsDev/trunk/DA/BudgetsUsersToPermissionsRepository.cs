﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class BudgetsUsersToPermissionsRepository : BaseRepository<Budgets_UsersToPermissions, Entities>, IDisposable
    {
        public override bool Create(Budgets_UsersToPermissions entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
