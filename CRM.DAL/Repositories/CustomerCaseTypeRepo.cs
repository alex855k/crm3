namespace CRM.DAL.Repositories
{
    public class CustomerCaseTypeRepo<T> : BaseRepository<T> where T : class
    {
        public CustomerCaseTypeRepo(CRMContext context) : base(context)
        {

        }
    }
}
