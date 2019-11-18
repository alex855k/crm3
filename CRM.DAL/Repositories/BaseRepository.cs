using CRM.DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using CRM.Models;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly CRMContext _db;
        private readonly DbSet<T> _table;

        public BaseRepository(CRMContext context)
        {
            _db = context;
            _table = _db.Set<T>();
        }
        public T Find(object id)
        {
            return _table.Find(id);
        }
        public async Task<T> FindAsync(object id)
        {
            return await _table.FindAsync(id);
        }
        public virtual void Add(T obj)
        {
            _table.Attach(obj);
            _table.Add(obj);
        }
        public void AddRange(List<T> listObj)
        {
            _table.AddRange(listObj);
        }

        public void Update(T obj)
        {
            _db.Entry(obj).State = EntityState.Modified;
        }
        public void Remove(object id)
        {
            T existing = _table.Find(id);
            _table.Remove(existing);
        }
        public void RemoveObject(T obj)
        {
            _table.Remove(obj);
        }
        public void RemoveRange(List<T> listObj)
        {
            _table.RemoveRange(listObj);
        }
        public virtual IEnumerable<T> Search(Expression<Func<T, bool>> predicate)
        {
            return _db.Set<T>().Where(predicate);
        }

        public virtual IQueryable<T> SearchQueryable(Expression<Func<T, bool>> predicate, params string[] includes)
        {
            IQueryable<T> queryable = _db.Set<T>();
            foreach (var include in includes)
            {
                queryable = queryable.Include(include);
            }
            return _db.Set<T>().Where(predicate);
        }
        public virtual IQueryable<T> AsQueryable()
        {
            return _db.Set<T>().AsQueryable();
        }
        public virtual IEnumerable<T> SearchInclude(Expression<Func<T, bool>> predicate, params string[] includes)
        {
        
            IQueryable<T> queryable = _db.Set<T>();
            foreach (var include in includes)
            {
                queryable = queryable.Include(include);
            }
            return queryable.Where(predicate);
        }

        public List<T> GetAllPagination(Expression<Func<T, bool>> predicate ,int pageIndex, int pageSize, string orderBy, string direction,params string[] includes)
        {
            IQueryable<T> queryable = _db.Set<T>();
            foreach (var include in includes)
            {
                queryable = queryable.Include(include);
            }
            return queryable.Where(predicate).OrderBy($"{orderBy} {direction}").Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }
        public List<T> GetAll()
        {
            return _db.Set<T>().ToList();
        }

        public List<T> GetAllPagination(Expression<Func<T, bool>> predicate, int pageIndex,int pageSize,string orderBy,string direction)
        {
            return _db.Set<T>().Where(predicate).OrderBy($"{orderBy} {direction}").Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }
        public List<T> GetAll(string Include)
        {
            return _db.Set<T>().Include(Include).ToList();
        }
        public void DeleteAll()
        {
            _db.Set<T>().RemoveRange(GetAll());
        }
        public DynamicTableQueryResult<T> DynamicTable(int pageSize, int pageIndex, string orderBy, string orderByDefault, string direction, Expression<Func<T, bool>> filter, params string[] includes)
        {
            DynamicTableQueryResult<T> dynamicTableQueryResult = new DynamicTableQueryResult<T>();
            var table = _db.Set<T>();
            IQueryable<T> queryable = table.AsQueryable();
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }
            if (includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    queryable = queryable.Include(include);
                }

            }
            var orderDirection = direction.ToLower() == "ASC".ToLower() ? "ascending" : "descending";
            if (!string.IsNullOrEmpty(orderBy))
            {
                string orderByQuery = string.Format("{0} {1}", orderBy, orderDirection);
                queryable = queryable.OrderBy(orderByQuery);
            }
            else
            {
                string orderByQuery = string.Format("{0} {1}", orderByDefault, orderDirection);
                queryable = queryable.OrderBy(orderByQuery);
            }
            dynamicTableQueryResult.QueryCount = queryable.Count();
            dynamicTableQueryResult.TableCount = table.Count();
            //dynamicTableQueryResult.QueryResultListAllResults = queryable.ToList();
            queryable = queryable.Skip(pageIndex * pageSize).Take(pageSize);
            dynamicTableQueryResult.QueryResultList = queryable.ToList();
            return dynamicTableQueryResult;
        }

        public List<U> Get<U>(Expression<Func<T, U>> select, Expression<Func<T, bool>> where) where U : class
        {
            return _table.Where(where).Select(select).ToList();
        }
        public List<U> Get<U>(Expression<Func<T, U>> select) where U : class
        {
            return _table.Select(select).ToList();
        }
    }

}
