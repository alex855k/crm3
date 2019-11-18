namespace CRM.DAL.Repositories
{
    public class ProductRepo<T> : BaseRepository<T> where T : class
    {
        public ProductRepo(CRMContext context) : base(context)
        {

        }
    }
}
