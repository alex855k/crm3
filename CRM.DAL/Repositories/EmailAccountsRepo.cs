namespace CRM.DAL.Repositories
{
    public class EmailAccountsRepo<T> : BaseRepository<T> where T : class
    {
        public EmailAccountsRepo(CRMContext context) : base(context)
        {
        }
    }
}
