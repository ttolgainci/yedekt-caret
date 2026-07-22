using MarbleWebProject.Models;

namespace MarbleWebProject.Helpers;

public static class PaymentReceiptUploadHelper
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"];

    public static async Task<(bool Ok, string? Path, string? Error)> SaveAsync(
        IWebHostEnvironment env,
        IConfiguration configuration,
        int orderId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return (false, null, "Dosya seçilmedi.");

        if (file.Length > 10 * 1024 * 1024)
            return (false, null, "Dosya en fazla 10 MB olabilir.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return (false, null, "Yalnızca JPG, PNG, GIF, WEBP veya PDF yükleyebilirsiniz.");

        var project = configuration["StoreAuth:ProjectName"]?.Trim();
        if (string.IsNullOrWhiteSpace(project))
            project = AppConfig.ProjectName;
        if (string.IsNullOrWhiteSpace(project))
            project = "Store";

        var folder = $"{project}/Upload/payment-receipts";
        var physicalDir = Path.Combine(env.WebRootPath, folder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(physicalDir);

        var storedName = $"order-{orderId}-{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(physicalDir, storedName);
        await using (var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = $"{folder}/{storedName}".Replace('\\', '/');
        return (true, relativePath, null);
    }
}
