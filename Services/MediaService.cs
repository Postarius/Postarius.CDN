using System.Collections.Generic;
using Data;
using Domain;

namespace Services
{
    public interface IMediaService
    {
        void Save(Media media);
        void Delete(int id);
        void Delete(IEnumerable<int> ids);
        void ChangeState(int id, MediaState newState);
    }
    
    public class MediaService : IMediaService
    {
        private IMediaRepository MediaRepository { get; }

        public MediaService(IMediaRepository mediaRepository)
        {
            MediaRepository = mediaRepository;
        }

        public void Save(Media media)
        {
            MediaRepository.Save(media);
        }

        public void Delete(int id)
        {
            MediaRepository.Delete(id);
        }

        public void Delete(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                Delete(id);
            }
        }

        public void ChangeState(int id, MediaState newState)
        {
            var media = MediaRepository.GetById(id);
            media.State = newState;
            MediaRepository.Save(media);
        }
    }
}