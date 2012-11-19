using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Objects;
using System.Data;
using System.Data.Entity;

namespace BaseLibraries
{
    public abstract class BaseRepository<TEntity, TContext> : IDisposable
        where TEntity : EntityObject, new()
        where TContext : ObjectContext, new()
    {
        protected TContext _db;

        public BaseRepository()
        {
            _db = new TContext();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public virtual IQueryable<TEntity> GetList(params string[] includes)
        {
            try
            {
                ObjectQuery<TEntity> objectSet = _db.CreateObjectSet<TEntity>();
                
                foreach (string include in includes)
                {
                    objectSet = objectSet.Include(include);
                }

                return objectSet;
            }
            catch
            {
                return Enumerable.Empty<TEntity>().AsQueryable();
            }
        }

        public virtual bool Create(TEntity entity)
        {
            try
            {
                _db.CreateObjectSet<TEntity>().AddObject(entity);
                _db.SaveChanges();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public virtual bool AddList(List<TEntity> list)
        {
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    _db.CreateObjectSet<TEntity>().AddObject(list[i]);
                }
            }
            catch
            {
                return false;
            }
            _db.SaveChanges();
            return true;
        }

        public TEntity GetEntity(int id, params string[] includes)
        {
            try
            {
                ObjectQuery<TEntity> objectSet = _db.CreateObjectSet<TEntity>();

                foreach (string include in includes)
                {
                    objectSet = objectSet.Include(include);
                }

                return objectSet.SingleOrDefault(GetEntityQuery(id));
            }
            catch
            {
                return null;
            }
        }

        public virtual TEntity Update(TEntity Entity, bool saveChanges = true)
        {
            try
            {
                var entitySet = _db.CreateObjectSet<TEntity>().EntitySet;
                var fqen = string.Format("{0}.{1}", entitySet.EntityContainer, entitySet.Name);
                if (Entity.EntityState != EntityState.Modified)
                {
                    var primaryKeyName = GetPrimaryKeyName(typeof(TEntity));
                    var primaryKeyValue = (int)typeof(TEntity).GetProperty(primaryKeyName).GetValue(Entity, null);
                    var StubEntity = GetEntity(primaryKeyValue);
                    if (StubEntity.EntityState == EntityState.Detached)
                    {
                        var entityInContext = _db.GetObjectByKey(StubEntity.EntityKey);

                        if (entityInContext == null)
                            _db.AttachTo(fqen, Entity);
                        else
                            Entity = (TEntity)entityInContext;
                    }
                }

                _db.ApplyCurrentValues(fqen, Entity);
                _db.SaveChanges();
                return Entity;
            }
            catch
            {
                return null;
            }
        }

        public virtual bool Delete(int id)
        {
            try
            {
                TEntity entityToDelete = _db.CreateObjectSet<TEntity>().SingleOrDefault(GetEntityQuery(id));
                _db.DeleteObject(entityToDelete);
                _db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        private Expression<Func<TEntity, bool>> GetEntityQuery(int id)
        {
            ParameterExpression e = Expression.Parameter(typeof(TEntity), "e");
            PropertyInfo propinfo = typeof(TEntity).GetProperty(GetPrimaryKeyName(typeof(TEntity)));
            MemberExpression m = Expression.MakeMemberAccess(e, propinfo);
            ConstantExpression c = Expression.Constant(id, typeof(int));
            BinaryExpression b = Expression.Equal(m, c);
            Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(b, e);
            return lambda;
        }

        private static string GetPrimaryKeyName(Type type)
        {
            string tempPrimaryKeyName = "";
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {

                foreach (var attr in prop.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false))
                {
                    EdmScalarPropertyAttribute edmProp = (EdmScalarPropertyAttribute)attr;

                    if (edmProp.EntityKeyProperty)
                    {
                        tempPrimaryKeyName = prop.Name;
                        break;
                    }
                    if (!String.IsNullOrEmpty(tempPrimaryKeyName))

                        break;
                }
                if (!String.IsNullOrEmpty(tempPrimaryKeyName))

                    break;
            }
            return tempPrimaryKeyName;
            //return typeof(TEntity).GetProperties().Where(property => ((EdmScalarPropertyAttribute)property.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false).FirstOrDefault()).EntityKeyProperty).FirstOrDefault().Name;        
        }


    }
}
