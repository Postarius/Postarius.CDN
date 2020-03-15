namespace Data
{
    public interface IUnitOfWork
    {
        void Commit();
    }
    
    public class UnitOfWork : IUnitOfWork
    {
        private PostariusCdnContext DbContext { get; }

        public UnitOfWork(PostariusCdnContext dbContext)
        {
            DbContext = dbContext;
        }

        public void Commit()
        {
            DbContext.SaveChanges();
        }
    }
}