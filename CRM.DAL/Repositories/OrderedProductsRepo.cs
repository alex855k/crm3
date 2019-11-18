namespace CRM.DAL.Repositories
{
    public class OrderItemRepo<T> : BaseRepository<T> where T : class
    {
        public OrderItemRepo(CRMContext context) : base(context)
        {
        }
    }
}
