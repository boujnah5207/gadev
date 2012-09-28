using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Objects;
using System.Data;

namespace BaseLibraries
{
    public static class ExtensionMethods
    {
        public static IEnumerable<TEntity> SelectPage<TEntity>(this IEnumerable<TEntity> entities, int itemsInPage, int pageNum)
        {
            return entities.Skip(itemsInPage * pageNum - 1).Take(itemsInPage);
        }
    }
}
