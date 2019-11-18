namespace CRM.DAL.Repositories
{
    public class CustomerNotesRepo<T> : BaseRepository<T> where T : class
    {
        public CustomerNotesRepo(CRMContext context) : base(context)
        {
        }
    }
}
