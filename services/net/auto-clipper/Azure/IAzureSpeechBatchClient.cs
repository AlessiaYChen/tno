using System;
using System.Threading;
using System.Threading.Tasks;
using TNO.Services.AutoClipper.Azure.Models;

namespace TNO.Services.AutoClipper.Azure;

public interface IAzureSpeechBatchClient
{
    Task<BatchTranscription> CreateTranscriptionAsync(string displayName, string locale, Uri contentUri, AzureSpeechBatchOptions batchOptions, CancellationToken cancellationToken);

    Task<BatchTranscription> GetTranscriptionAsync(string transcriptionId, CancellationToken cancellationToken);

    Task<BatchTranscriptionFilesResponse> GetFilesAsync(string transcriptionId, CancellationToken cancellationToken);

    Task<string> DownloadContentAsync(Uri contentUri, CancellationToken cancellationToken);
}

public class AzureSpeechBatchOptions
{
    public bool WordLevelTimestampsEnabled { get; set; }

    public bool DiarizationEnabled { get; set; }

    public int? MinSpeakers { get; set; }

    public int? MaxSpeakers { get; set; }

    public string? DiarizationMode { get; set; }

    public string ProfanityFilterMode { get; set; } = "Masked";

    public string PunctuationMode { get; set; } = "DictatedAndAutomatic";
}
