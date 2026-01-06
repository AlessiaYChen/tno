using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Config;

namespace TNO.Services.AutoClipper.Azure;

public class AzureBlobStagingService : IAzureBlobStagingService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AutoClipperOptions _options;
    private readonly ILogger<AzureBlobStagingService> _logger;

    public AzureBlobStagingService(IOptions<AutoClipperOptions> options, ILogger<AzureBlobStagingService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_options.AzureBlobConnectionString))
            throw new InvalidOperationException("Azure blob connection string is not configured.");

        _blobServiceClient = new BlobServiceClient(_options.AzureBlobConnectionString);
    }

    public async Task<BlobStagingResult> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        var container = await GetContainerAsync(cancellationToken).ConfigureAwait(false);
        var blobName = BuildBlobName(fileName);
        var blobClient = container.GetBlobClient(blobName);

        if (content.CanSeek) content.Position = 0;

        await blobClient.UploadAsync(content, overwrite: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Uploaded file {FileName} to Azure blob {BlobName}", fileName, blobName);

        var sasUri = GenerateReadOnlySas(blobClient);
        return new BlobStagingResult(container.Name, blobName, blobClient.Uri, sasUri);
    }

    public async Task DeleteAsync(BlobStagingResult stagingResult, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(stagingResult?.BlobName) || string.IsNullOrWhiteSpace(stagingResult?.ContainerName)) return;

        var container = _blobServiceClient.GetBlobContainerClient(stagingResult.ContainerName);
        await container.DeleteBlobIfExistsAsync(stagingResult.BlobName, DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<BlobContainerClient> GetContainerAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.AzureBlobInputContainer))
            throw new InvalidOperationException("Azure blob input container is not configured.");

        var container = _blobServiceClient.GetBlobContainerClient(_options.AzureBlobInputContainer);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken).ConfigureAwait(false);
        return container;
    }

    private Uri GenerateReadOnlySas(BlobClient blobClient)
    {
        if (!blobClient.CanGenerateSasUri)
            throw new InvalidOperationException("Blob client cannot generate SAS URIs. Use a connection string with account key.");

        var validityMinutes = Math.Max(5, _options.AzureBlobSasValidityMinutes);
        var builder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(validityMinutes)
        };
        builder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(builder);
    }

    private static string BuildBlobName(string fileName)
    {
        var safeName = string.IsNullOrWhiteSpace(fileName) ? "audio" : Path.GetFileName(fileName)!;
        return $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}-{safeName}";
    }
}
