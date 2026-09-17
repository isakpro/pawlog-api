namespace Pawlog.Api.Services;

public class PhotoStorage
{
    public const string RequestPath = "/uploads";

    private const long MaxBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public PhotoStorage(IWebHostEnvironment environment)
    {
        RootPath = Path.Combine(environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public string? Validate(IFormFile photo)
    {
        if (photo.Length == 0)
        {
            return "The photo is empty.";
        }

        if (photo.Length > MaxBytes)
        {
            return "The photo can be at most 5 MB.";
        }

        if (!AllowedExtensions.Contains(Path.GetExtension(photo.FileName).ToLowerInvariant()))
        {
            return "The photo must be a jpg, png, webp or gif image.";
        }

        return null;
    }

    public async Task<string> SaveAsync(IFormFile photo)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName).ToLowerInvariant()}";

        await using var stream = File.Create(Path.Combine(RootPath, fileName));
        await photo.CopyToAsync(stream);

        return $"{RequestPath}/{fileName}";
    }

    public void Delete(string? photoUrl)
    {
        if (photoUrl is null)
        {
            return;
        }

        File.Delete(Path.Combine(RootPath, Path.GetFileName(photoUrl)));
    }
}
