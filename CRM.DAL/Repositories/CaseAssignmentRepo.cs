namespace CRM.DAL.Repositories
{
    public class CaseAssignmentRepo<T> : BaseRepository<T> where T : class
    {
        public CaseAssignmentRepo(CRMContext context) : base(context)
        {

        }
    }
}