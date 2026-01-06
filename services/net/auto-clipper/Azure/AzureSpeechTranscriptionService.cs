using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Azure.Models;
using TNO.Services.AutoClipper.Config;

namespace TNO.Services.AutoClipper.Azure;

public class AzureSpeechTranscriptionService : IAzureSpeechTranscriptionService
{
    private readonly AutoClipperOptions _options;
    private readonly ILogger<AzureSpeechTranscriptionService> _logger;
    private readonly IAzureBlobStagingService _blobStagingService;
    private readonly IAzureSpeechBatchClient _batchClient;
    private readonly SemaphoreSlim _batchSemaphore;

    public AzureSpeechTranscriptionService(
        IOptions<AutoClipperOptions> options,
        ILogger<AzureSpeechTranscriptionService> logger,
        IAzureBlobStagingService blobStagingService,
        IAzureSpeechBatchClient batchClient)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _blobStagingService = blobStagingService ?? throw new ArgumentNullException(nameof(blobStagingService));
        _batchClient = batchClient ?? throw new ArgumentNullException(nameof(batchClient));

        var maxConcurrent = Math.Max(1, _options.AzureSpeechBatchMaxConcurrentJobs);
        _batchSemaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<IReadOnlyList<TimestampedTranscript>> TranscribeAsync(string filePath, SpeechTranscriptionRequest request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (!File.Exists(filePath)) throw new FileNotFoundException("Audio file does not exist", filePath);
        if (string.IsNullOrWhiteSpace(_options.AzureSpeechKey) ||
            (string.IsNullOrWhiteSpace(_options.AzureSpeechRegion) && string.IsNullOrWhiteSpace(_options.AzureSpeechEndpoint)))
            throw new InvalidOperationException("Azure Speech configuration is missing.");

        var attempts = Math.Max(1, _options.AzureSpeechMaxRetries);
        var retryDelay = TimeSpan.FromSeconds(Math.Max(1, _options.AzureSpeechRetryDelaySeconds));
        Exception? lastError = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            try
            {
                return await SubmitBatchJobAsync(filePath, request, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastError = ex;
                if (attempt >= attempts) throw;
                _logger.LogWarning(ex, "Azure Speech batch transcription attempt {Attempt}/{Attempts} failed for {File}. Retrying in {Delay}...", attempt, attempts, filePath, retryDelay);
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        throw lastError ?? new InvalidOperationException("Azure Speech transcription failed unexpectedly.");
    }

    private async Task<IReadOnlyList<TimestampedTranscript>> SubmitBatchJobAsync(string filePath, SpeechTranscriptionRequest request, CancellationToken cancellationToken)
    {
        await _batchSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        BlobStagingResult? stagedBlob = null;

        try
        {
            await using var stream = File.OpenRead(filePath);
            stagedBlob = await _blobStagingService.UploadAsync(Path.GetFileName(filePath), stream, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Uploaded {File} to Azure blob {Blob}", filePath, stagedBlob.BlobName);

            var locale = !string.IsNullOrWhiteSpace(request.Language)
                ? request.Language
                : string.IsNullOrWhiteSpace(_options.DefaultTranscriptLanguage)
                    ? "en-US"
                    : _options.DefaultTranscriptLanguage;

            var desiredDiarization = request.EnableSpeakerDiarization || _options.AzureSpeechBatchDiarizationEnabled;
            var diarizationEnabled = desiredDiarization && request.SpeakerCount.GetValueOrDefault() > 0;
            if (desiredDiarization && !diarizationEnabled)
            {
                _logger.LogWarning("Speaker diarization requested but no speaker count was provided. Falling back to diarization disabled.");
            }

            var batchOptions = new AzureSpeechBatchOptions
            {
                WordLevelTimestampsEnabled = _options.AzureSpeechBatchWordLevelTimestampsEnabled,
                DiarizationEnabled = diarizationEnabled,
                MinSpeakers = diarizationEnabled ? request.MinSpeakerCount : null,
                MaxSpeakers = diarizationEnabled ? request.SpeakerCount : null,
                DiarizationMode = diarizationEnabled ? request.DiarizationMode : null,
                ProfanityFilterMode = _options.AzureSpeechBatchProfanityFilterMode,
                PunctuationMode = _options.AzureSpeechBatchPunctuationMode
            };

            var transcription = await _batchClient.CreateTranscriptionAsync(
                $"autoclipper-{DateTime.UtcNow:yyyyMMddHHmmss}",
                locale,
                stagedBlob.ReadOnlyUri,
                batchOptions,
                cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(transcription.Id))
                throw new InvalidOperationException("Azure Speech returned a transcription without an identifier.");

            return await PollUntilCompleteAsync(transcription, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (stagedBlob != null && _options.AzureSpeechBatchDeleteInputOnCompletion)
            {
                try
                {
                    await _blobStagingService.DeleteAsync(stagedBlob, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete staged blob {Blob}", stagedBlob.BlobName);
                }
            }

            _batchSemaphore.Release();
        }
    }

    private async Task<IReadOnlyList<TimestampedTranscript>> PollUntilCompleteAsync(BatchTranscription transcription, CancellationToken cancellationToken)
    {
        var pollDelay = TimeSpan.FromSeconds(Math.Max(5, _options.AzureSpeechBatchPollIntervalSeconds));
        var timeout = TimeSpan.FromMinutes(Math.Max(1, _options.AzureSpeechBatchTimeoutMinutes));
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        var current = transcription;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.Equals(current.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
            {
                return await DownloadTranscriptAsync(current.Id!, cancellationToken).ConfigureAwait(false);
            }

            if (string.Equals(current.Status, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                var reason = current.Properties?.Error?.Message ?? "Azure Speech batch transcription failed.";
                throw new InvalidOperationException(reason);
            }

            if (DateTimeOffset.UtcNow >= deadline)
                throw new TimeoutException($"Azure Speech batch transcription timed out after {timeout.TotalMinutes:F0} minutes.");

            await Task.Delay(pollDelay, cancellationToken).ConfigureAwait(false);
            current = await _batchClient.GetTranscriptionAsync(current.Id!, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<TimestampedTranscript>> DownloadTranscriptAsync(string transcriptionId, CancellationToken cancellationToken)
    {
        var files = await _batchClient.GetFilesAsync(transcriptionId, cancellationToken).ConfigureAwait(false);
        var transcriptFile = files.Values
            .Where(f => f.Kind != null)
            .OrderByDescending(f => f.Name?.EndsWith(".json", StringComparison.OrdinalIgnoreCase) == true)
            .ThenByDescending(f => string.Equals(f.Kind, "Transcription", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var downloadUri = transcriptFile?.ContentUrl ?? transcriptFile?.Links?.ContentUrl;
        if (downloadUri == null)
        {
            _logger.LogWarning("Azure Speech files listing did not contain a downloadable transcript. Files: {files}",
                string.Join(", ", files.Values.Select(f => $"{f.Kind}:{f.Name}")));
            throw new InvalidOperationException($"No transcript file was available for transcription '{transcriptionId}'.");
        }

        var payload = await _batchClient.DownloadContentAsync(downloadUri, cancellationToken).ConfigureAwait(false);
        return ParseTranscriptSegments(payload);
    }

    private IReadOnlyList<TimestampedTranscript> ParseTranscriptSegments(string payload)
    {
        var segments = new List<TimestampedTranscript>();
        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.TryGetProperty("recognizedPhrases", out var phrases) && phrases.ValueKind == JsonValueKind.Array)
        {
            foreach (var phrase in phrases.EnumerateArray())
            {
                var text = ExtractText(phrase);
                if (string.IsNullOrWhiteSpace(text)) continue;

                var start = ParseDuration(phrase, "offset");
                var duration = ParseDuration(phrase, "duration");
                var end = start + duration;
                var speaker = phrase.TryGetProperty("speaker", out var speakerValue) && speakerValue.ValueKind == JsonValueKind.Number
                    ? speakerValue.GetInt32().ToString()
                    : null;
                segments.Add(new TimestampedTranscript(start, end, text, speaker));
            }
        }

        if (segments.Count == 0 && document.RootElement.TryGetProperty("combinedRecognizedPhrases", out var combined) && combined.ValueKind == JsonValueKind.Array)
        {
            var fullText = combined.EnumerateArray()
                .Select(ExtractText)
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
            if (!string.IsNullOrWhiteSpace(fullText))
            {
                segments.Add(new TimestampedTranscript(TimeSpan.Zero, TimeSpan.Zero, fullText));
            }
        }

        return segments;
    }

    private static string? ExtractText(JsonElement phrase)
    {
        if (!phrase.TryGetProperty("nBest", out var nBest) || nBest.ValueKind != JsonValueKind.Array) return null;
        var first = nBest.EnumerateArray().FirstOrDefault();
        if (first.ValueKind == JsonValueKind.Undefined) return null;
        if (first.TryGetProperty("display", out var display) && display.ValueKind == JsonValueKind.String)
            return display.GetString();
        if (first.TryGetProperty("lexical", out var lexical) && lexical.ValueKind == JsonValueKind.String)
            return lexical.GetString();
        return null;
    }

    private static TimeSpan ParseDuration(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value)) return TimeSpan.Zero;
        if (value.ValueKind == JsonValueKind.Number)
        {
            return TimeSpan.FromTicks(value.GetInt64());
        }
        if (value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (string.IsNullOrWhiteSpace(text)) return TimeSpan.Zero;
            if (TimeSpan.TryParse(text, out var ts)) return ts;
            try
            {
                return System.Xml.XmlConvert.ToTimeSpan(text);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
        return TimeSpan.Zero;
    }
}
