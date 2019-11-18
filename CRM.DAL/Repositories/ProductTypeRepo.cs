namespace CRM.DAL.Repositories
{
    public class ProductTypeRepo<T> : BaseRepository<T> where T : class
    {
        public ProductTypeRepo(CRMContext context) : base(context)
        {

        }
    }
}