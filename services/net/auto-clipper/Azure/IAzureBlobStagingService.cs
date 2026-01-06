using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TNO.Services.AutoClipper.Azure;

public interface IAzureBlobStagingService
{
    Task<BlobStagingResult> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken);

    Task DeleteAsync(BlobStagingResult stagingResult, CancellationToken cancellationToken);
}

public record BlobStagingResult(string ContainerName, string BlobName, Uri BlobUri, Uri ReadOnlyUri);
