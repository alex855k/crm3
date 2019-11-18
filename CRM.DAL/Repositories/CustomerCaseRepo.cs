namespace CRM.DAL.Repositories
{
    public class CustomerCaseRepo<T> : BaseRepository<T> where T : class
    {
        public CustomerCaseRepo(CRMContext context) : base(context)
        {

        }
    }
}