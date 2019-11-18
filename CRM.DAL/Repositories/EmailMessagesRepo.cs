namespace CRM.DAL.Repositories
{
    public class EmailMessagesRepo<T> : BaseRepository<T> where T : class
    {
        public EmailMessagesRepo(CRMContext context) : base(context)
        {
        }
    }
}
