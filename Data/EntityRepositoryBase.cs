using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Common;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public interface IEntityRepository<TEntity> where TEntity : EntityBase
    {
        void Save(TEntity entity);
        void Save(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        void Delete(IEnumerable<TEntity> entities);
        void Delete(int id);
        void Delete(Expression<Func<TEntity, bool>> where);

        TEntity GetById(int id, bool throwExceptionIfNotFound = true);
        TResult GetById<TResult>(int id, Expression<Func<TEntity, TResult>> selector, bool throwIfNotFound = true);

        IQueryable<TEntity> GetAll();
        IQueryable<TResult> GetAll<TResult>(Expression<Func<TEntity, TResult>> selector);
        IQueryable<TEntity> GetMany(Expression<Func<TEntity, bool>> where);
        IQueryable<TResult> GetMany<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector);

        TEntity FirstOrDefault();
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> where);
        TEntity First();
        TEntity First(Expression<Func<TEntity, bool>> where);
        TEntity SingleOrDefault();
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> where);
        TEntity Single();
        TEntity Single(Expression<Func<TEntity, bool>> where);

        TResult FirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector);
        TResult FirstOrDefault<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector);
        TResult First<TResult>(Expression<Func<TEntity, TResult>> selector);
        TResult First<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector);
        TResult SingleOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector);
        TResult SingleOrDefault<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector);
        TResult Single<TResult>(Expression<Func<TEntity, TResult>> selector);
        TResult Single<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector);

        bool Any(Expression<Func<TEntity, bool>> where);
        bool Any();
        bool Exists(int id);
        int Count(Expression<Func<TEntity, bool>> where = null);

        IQueryable<TEntity> AsNoTracking();
    }
    
    public class EntityRepositoryBase<TEntity> : IEntityRepository<TEntity> where TEntity : EntityBase
    {
        private readonly DbSet<TEntity> dbSet;
        
        protected EntityRepositoryBase(PostariusCdnContext dbContext)
        {
            dbSet = dbContext.Set<TEntity>();
        }
        
        public virtual void Save(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentException(nameof(entity));

            if (entity.IsNew)
                Add(entity);
            else
                Update(entity);
        }

        protected virtual void Add(TEntity entity)
        {
            if (entity.CreatedAt.IsEmpty())
                entity.CreatedAt = DateTime.UtcNow;

            dbSet.Add(entity);
        }

        protected virtual void Update(TEntity entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }

        public void Save(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Save(entity);
            }
        }

        public void Delete(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        public void Delete(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }

        public void Delete(int id)
        {
            Delete(GetById(id));
        }

        public void Delete(Expression<Func<TEntity, bool>> where)
        {
            Delete(GetMany(where).ToArray());
        }

        public TEntity GetById(int id, bool throwExceptionIfNotFound = true)
        {
            return throwExceptionIfNotFound ? GetMany(e => e.Id == id).Single() : GetMany(e => e.Id == id).SingleOrDefault();
        }

        public TResult GetById<TResult>(int id, Expression<Func<TEntity, TResult>> selector, bool throwIfNotFound = true)
        {
            return throwIfNotFound ? GetMany(e => e.Id == id, selector).Single() :
                GetMany(e => e.Id == id, selector).SingleOrDefault();
        }

        public IQueryable<TEntity> GetAll()
        {
            return dbSet;
        }

        public IQueryable<TResult> GetAll<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Select(selector);
        }

        public IQueryable<TEntity> GetMany(Expression<Func<TEntity, bool>> where)
        {
            return dbSet.Where(where);
        }

        public IQueryable<TResult> GetMany<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Where(where).Select(selector);
        }

        public TEntity FirstOrDefault()
        {
            return dbSet.FirstOrDefault();
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> where)
        {
            return dbSet.FirstOrDefault(where);
        }

        public TEntity First()
        {
            return dbSet.First();
        }

        public TEntity First(Expression<Func<TEntity, bool>> where)
        {
            return dbSet.First(where);
        }

        public TEntity SingleOrDefault()
        {
            return dbSet.SingleOrDefault();
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> where)
        {
            return dbSet.SingleOrDefault(where);
        }

        public TEntity Single()
        {
            return dbSet.Single();
        }

        public TEntity Single(Expression<Func<TEntity, bool>> where)
        {
            return dbSet.Single(where);
        }

        public TResult FirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Select(selector).FirstOrDefault();
        }

        public TResult FirstOrDefault<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Where(where).Select(selector).FirstOrDefault();
        }

        public TResult First<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Select(selector).First();
        }

        public TResult First<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Where(where).Select(selector).First();
        }

        public TResult SingleOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Select(selector).SingleOrDefault();
        }

        public TResult SingleOrDefault<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Where(where).Select(selector).SingleOrDefault();
        }

        public TResult Single<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Select(selector).Single();
        }

        public TResult Single<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return dbSet.Where(where).Select(selector).Single();
        }

        public bool Any(Expression<Func<TEntity, bool>> where)
        {
            return dbSet.Where(where).Any();
        }

        public bool Any()
        {
            return dbSet.Any();
        }

        public bool Exists(int id)
        {
            return dbSet.Any(e => e.Id == id);
        }

        public int Count(Expression<Func<TEntity, bool>> where = null)
        {
            return where == null ? dbSet.Count() : dbSet.Count(where);
        }

        public IQueryable<TEntity> AsNoTracking()
        {
            return dbSet.AsNoTracking();
        }
    }
}