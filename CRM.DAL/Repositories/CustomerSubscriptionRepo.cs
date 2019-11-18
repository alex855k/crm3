namespace CRM.DAL.Repositories
{
    public class CustomerSubscriptionRepo<T> : BaseRepository<T> where T : class
    {
        public CustomerSubscriptionRepo(CRMContext context) : base(context)
        {

        }
    }
}