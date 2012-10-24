using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibraries;
using DB;

namespace DA
{
    public class SubProjectsRepository : BaseRepository<Projects_SubProject, Entities>, IDisposable
    {
        public override bool Create(Projects_SubProject entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
