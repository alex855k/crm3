namespace CRM.DAL.IRepository
{
    public interface IBaseRepository<T> where T : class
    {
        T Find(object id);
        void Add(T obj);
        void Update(T obj);
        void Remove(object id);
    }
}
