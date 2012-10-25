using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLibraries;
using DB;

namespace DA
{
    public class LanguagesRepository : BaseRepository<Language, Entities>, IDisposable
    {
    }
}
