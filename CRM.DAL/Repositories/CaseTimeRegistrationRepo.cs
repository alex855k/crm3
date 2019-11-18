namespace CRM.DAL.Repositories
{
    public class CaseTimeRegistrationRepo<T> : BaseRepository<T> where T : class
    {
        public CaseTimeRegistrationRepo(CRMContext context) : base(context)
        {

        }
    }
}