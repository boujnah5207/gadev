using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibraries;
using DB;

namespace DA
{
    public class ParentProjectsRepository : BaseRepository<Projects_ParentProject, Entities>, IDisposable
    {
        public override bool Create(Projects_ParentProject entity)
        {
            entity.CreationDate = DateTime.Now;
            return base.Create(entity);
        }
    }
}
