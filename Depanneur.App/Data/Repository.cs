using System;
using System.Linq;
using System.Linq.Expressions;
using Depanneur.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Depanneur.App.Data
{
    public abstract class Repository<T> where T : class
    {
        protected readonly DepanneurContext db;

        protected Repository(DepanneurContext db)
        {
            this.db = db;
        }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[] includes) 
        { 
            IQueryable<T> query = db.Set<T>();

            if (includes != null) 
            {
                foreach (var expr in includes)
                    query = query.Include(expr);
            }

            return query;
        }
        public T Get(object id) => db.Set<T>().Find(id);

        public T Add(T entity)
        {
            db.Set<T>().Add(entity);
            db.SaveChanges();

            return entity;
        }

        public T DeleteById(object id) 
        {
            return Delete(Get(id));
        }

        public T Delete(T entity)
        {
            if (entity is ISoftDeletable sd)
            {
                sd.IsDeleted = true;
            }
            else
            {
                db.Set<T>().Remove(entity);
            }

            db.SaveChanges();

            return entity;
        }

        public void Save() => db.SaveChanges();
    }

    public static class RepositoryExtensions
    {
        public static IQueryable<T> GetActive<T>(this Repository<T> repo) where T : class, ISoftDeletable
        {
            return repo.GetAll().Where(x => !x.IsDeleted);
        }

        public static T UndeleteById<T>(this Repository<T> repo, object id) where T : class, ISoftDeletable 
        {
            return repo.Undelete(repo.Get(id));
        }

        public static T Undelete<T>(this Repository<T> repo, T entity) where T : class, ISoftDeletable 
        {
            entity.IsDeleted = false;
            repo.Save();

            return entity;
        }
    }
}