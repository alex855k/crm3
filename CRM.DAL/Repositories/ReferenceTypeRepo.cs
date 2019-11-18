namespace CRM.DAL.Repositories
{
    public class ReferenceTypeRepo<T> : BaseRepository<T> where T : class
    {
        public ReferenceTypeRepo(CRMContext context) : base(context)
        {
        }
    }
}