using System;
using System.IO;
using Data;
using Domain;
using Hangfire;
using Services.BackgroundJobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Services
{
    public interface IImageService
    {
        ImagesMetadata Save(Stream imageStream);
        Stream CropImage(Stream imageData, int width, int height);
    }

    public class ImagesMetadata
    {
        public string RawUrl { get; set; }
        public string Cropped150x150Url { get; set; }
        public string Cropped300x400Url { get; set; }
    }
    
    public class FileSystemImageService : IImageService
    {
        // TODO : Refactoring required: need to move to AppSettings.
        private const string AppUrl = "http://localhost:7000";
        
        private IMediaService MediaService { get; }
        private IUnitOfWork UnitOfWork { get; }

        public FileSystemImageService(IMediaService mediaService,
            IUnitOfWork unitOfWork)
        {
            MediaService = mediaService;
            UnitOfWork = unitOfWork;
        }
        
        public ImagesMetadata Save(Stream imageStream)
        {
            var fileName = Guid.NewGuid().ToString("N") + ".png";
            using var fileStream = File.Create(GetAbsolutePath(ResolveRawImageUrl(fileName)));
            imageStream.CopyTo(fileStream);

            var metadata = PreprocessImages(fileName);
            return metadata;
        }

        public Stream CropImage(Stream imageData, int width, int height)
        {
            var image = Image.Load(imageData);
            var ratio = (double)image.Width / image.Height;
            var initialRation = (double) width / height;

            var coefficient = ratio > initialRation ? (double) image.Height / height : (double) image.Width / width;
            var offsetX = (int)Math.Round(ratio > initialRation ? (ratio * height) * 0.5 - width * 0.5 : 0);
            var offsetY = (int)Math.Round(ratio > initialRation ? 0 : width * 0.5 / ratio - height * 0.5);

            var resizedWidth = (int)Math.Round(image.Width / coefficient);
            var resizedHeight = (int) Math.Round(image.Height / coefficient);
            image.Mutate(i => i
                .Resize(resizedWidth, resizedHeight)
                .Crop(new Rectangle(offsetX, offsetY, width, height)));
            
            var stream = new MemoryStream();
            image.Save(stream, new PngEncoder());
            stream.Position = 0;

            return stream;
        }

        private ImagesMetadata PreprocessImages(string fileName)
        {
            var rawPath = ResolveRawImageUrl(fileName);

            var media = new Media
            {
                RawPath = GetAbsolutePath(rawPath),
                State = MediaState.Unprocessed
            };
            
            MediaService.Save(media);
            UnitOfWork.Commit();

            BackgroundJob.Enqueue<ImageProcessingJob>(j => j.Execute());
            
            return new ImagesMetadata
            {
                RawUrl = $"{AppUrl}/{rawPath}",
                Cropped150x150Url = $"{AppUrl}/{ResolveCroppedImageUrl(fileName, 150, 150)}",
                Cropped300x400Url = $"{AppUrl}/{ResolveCroppedImageUrl(fileName, 300, 400)}"
            };
        }

        private string GetAbsolutePath(string relativePath)
        {
            return $"{Directory.GetCurrentDirectory()}/wwwroot/{relativePath}";
        }

        private string ResolveRawImageUrl(string imageFileName)
        {
            return $"Images/Raws/{imageFileName}";
        }

        private string ResolveCroppedImageUrl(string imageFileName, int width, int height)
        {
            return $"Images/{width}x{height}/{imageFileName}";
        }
    }
}