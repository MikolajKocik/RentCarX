using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RentCarX.Application.Interfaces.Services.File;

namespace RentCarX.Infrastructure.Services.CarImage;

public sealed class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<FileUploadService> _logger;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private const string ImagesFolder = "images/cars";
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public FileUploadService(IWebHostEnvironment webHostEnvironment, ILogger<FileUploadService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    private string GetRootPath()
    {
        return _webHostEnvironment.WebRootPath
               ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    /// <summary>
    /// Asynchronously uploads an image file and returns the relative URL of the stored image.
    /// </summary>
    /// <remarks>Only files with supported extensions and sizes up to 5 MB are accepted. The method generates
    /// a unique file name for each upload and stores the image in the configured images folder under the web
    /// root.</remarks>
    /// <param name="file">The image file to upload. Must not be <see langword="null"/> and must have a supported file extension and a
    /// non-zero length.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A relative URL string to the uploaded image if the upload succeeds; otherwise, <see langword="null"/> if the
    /// file is <see langword="null"/> or empty.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the file has an unsupported extension or exceeds the maximum allowed file size.</exception>
    public async Task<string?> UploadImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return null;

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Unsupported file extension: {Extension}", extension);
            throw new InvalidOperationException($"Unsupported file type: {extension}");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            _logger.LogWarning("File size exceeds limit: {Size}", file.Length);
            throw new InvalidOperationException($"File size exceeds 5 MB limit");
        }

        try
        {
            string rootPath = GetRootPath();
            string uploadsFolder = Path.Combine(rootPath, ImagesFolder);
            Directory.CreateDirectory(uploadsFolder);

            string fileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            string imageUrl = $"/{ImagesFolder}/{fileName}";
            _logger.LogInformation("Image uploaded successfully: {ImageUrl}", imageUrl);
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified image file from the application's web root folder asynchronously.
    /// </summary>
    /// <remarks>If the specified image file does not exist, the method completes without error. Any
    /// exceptions encountered during deletion are logged but not propagated to the caller.</remarks>
    /// <param name="imagePath">The relative or absolute path to the image file to delete. If the path is null or empty, the method does
    /// nothing.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteImageAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(imagePath))
            return;

        try
        {
            string fileName = Path.GetFileName(imagePath);

            string rootPath = GetRootPath();
            string filePath = Path.Combine(rootPath, ImagesFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Image deleted: {FilePath}", filePath);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImagePath}", imagePath);
        }
    }
}
