using Microsoft.AspNetCore.Http;

namespace RentCarX.Application.Interfaces.Services.File;

public interface IFileUploadService
{
    Task<string?> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string imagePath, CancellationToken cancellationToken = default);
}