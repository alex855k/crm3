namespace CRM.DAL.Repositories
{
    public class ReferencelistRepo<T> : BaseRepository<T> where T : class
    {
        public ReferencelistRepo(CRMContext context) : base(context)
        {
        }
    }
}
