using Domain;

namespace Data
{
    public interface IMediaRepository : IEntityRepository<Media>
    {
        
    }
    
    public class MediaRepository : EntityRepositoryBase<Media>, IMediaRepository
    {
        public MediaRepository(PostariusCdnContext dbContext) : base(dbContext)
        {
        }
    }
}