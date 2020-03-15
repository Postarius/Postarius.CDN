using System.IO;
using System.Linq;
using Data;
using Domain;
using Microsoft.Extensions.Logging;

namespace Services.BackgroundJobs
{
    public class ImageProcessingJob : IBackgroundJob
    {
        private readonly string relativeSavePath = Path.Combine("wwwroot", "Images");
        private readonly string cropped150x150path;
        private readonly string cropped300x400path;
        
        private IMediaRepository MediaRepository { get; }
        private IMediaService MediaService { get; }
        private IUnitOfWork UnitOfWork { get; }
        private IImageService ImageService { get; }
        private ILogger<ImageProcessingJob> Log { get; }

        public ImageProcessingJob(IMediaRepository mediaRepository,
            IMediaService mediaService,
            IUnitOfWork unitOfWork,
            IImageService imageService,
            ILogger<ImageProcessingJob> log)
        {
            MediaRepository = mediaRepository;
            MediaService = mediaService;
            UnitOfWork = unitOfWork;
            ImageService = imageService;
            Log = log;

            cropped300x400path = Path.Combine(relativeSavePath, "300x400");
            cropped150x150path = Path.Combine(relativeSavePath, "150x150");
        }
        
        public void Execute()
        {
            Log.LogInformation("ImageProcessingJob: Started image processing.");
            CleanupProcessedImages();
            var unprocessedImages = MediaRepository.GetMany(m => m.State == MediaState.Unprocessed).ToArray();

            foreach (var imageData in unprocessedImages)
            {
                using var rawImageStream = File.OpenRead(imageData.RawPath);
                var imageFileName = ResolveFileName(imageData.RawPath);

                var cropped150x150 = ImageService.CropImage(rawImageStream, 150, 150);
                Save(cropped150x150, $"{cropped150x150path}/{imageFileName}");

                rawImageStream.Position = 0;

                var cropped300x400 = ImageService.CropImage(rawImageStream, 300, 400);
                Save(cropped300x400, $"{cropped300x400path}/{imageFileName}");

                imageData.State = MediaState.Processed;
                MediaService.Save(imageData);
                UnitOfWork.Commit();
            }
        }

        private void Save(Stream imageStream, string path)
        {
            using var fileStream = File.Create(path);
            imageStream.Seek(0, SeekOrigin.Begin);
            imageStream.CopyTo(fileStream);
        }

        private void CleanupProcessedImages()
        {
            var processedImages = MediaRepository
                .GetMany(i => i.State == MediaState.Processed, i => i.Id)
                .ToArray();
            
            MediaService.Delete(processedImages);
            UnitOfWork.Commit();
        }

        private string ResolveFileName(string rawPath)
        {
            return Path.GetFileName(rawPath);
        }
    }
}