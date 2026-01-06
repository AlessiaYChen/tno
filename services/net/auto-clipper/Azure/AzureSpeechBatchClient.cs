using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Azure.Models;
using TNO.Services.AutoClipper.Config;

namespace TNO.Services.AutoClipper.Azure;

public class AzureSpeechBatchClient : IAzureSpeechBatchClient
{
    private const string TranscriptionsPath = "speechtotext/v3.2/transcriptions";

    private readonly HttpClient _httpClient;
    private readonly AutoClipperOptions _options;
    private readonly ILogger<AzureSpeechBatchClient> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public AzureSpeechBatchClient(HttpClient httpClient, IOptions<AutoClipperOptions> options, ILogger<AzureSpeechBatchClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConfigureHttpClient();
    }

    public async Task<BatchTranscription> CreateTranscriptionAsync(string displayName, string locale, Uri contentUri, AzureSpeechBatchOptions batchOptions, CancellationToken cancellationToken)
    {
        var payload = new BatchTranscriptionCreateRequest
        {
            DisplayName = displayName,
            Locale = locale,
            ContentUrls = new[] { contentUri.ToString() },
            Properties = new BatchTranscriptionCreateProperties
            {
                DiarizationEnabled = batchOptions.DiarizationEnabled,
                WordLevelTimestampsEnabled = batchOptions.WordLevelTimestampsEnabled,
                ProfanityFilterMode = batchOptions.ProfanityFilterMode,
                PunctuationMode = batchOptions.PunctuationMode,
                Diarization = batchOptions.DiarizationEnabled ? new BatchTranscriptionDiarization
                {
                    Speakers = new BatchTranscriptionDiarizationSpeakers
                    {
                        MinCount = batchOptions.MinSpeakers ?? 1,
                        MaxCount = batchOptions.MaxSpeakers
                    },
                    Mode = batchOptions.DiarizationMode
                } : null
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(TranscriptionsPath, payload, _serializerOptions, cancellationToken).ConfigureAwait(false);
        await ThrowIfFailedAsync(response, cancellationToken).ConfigureAwait(false);

        var transcription = await DeserializeAsync<BatchTranscription>(response, cancellationToken).ConfigureAwait(false);
        if (transcription?.Id?.Length > 0)
            return transcription;

        var transcriptionId = GetIdFromLocation(response.Headers.Location);
        if (string.IsNullOrWhiteSpace(transcriptionId))
            throw new InvalidOperationException("Azure Speech did not return a transcription identifier.");

        return await GetTranscriptionAsync(transcriptionId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BatchTranscription> GetTranscriptionAsync(string transcriptionId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"{TranscriptionsPath}/{transcriptionId}", cancellationToken).ConfigureAwait(false);
        await ThrowIfFailedAsync(response, cancellationToken).ConfigureAwait(false);
        var transcription = await DeserializeAsync<BatchTranscription>(response, cancellationToken).ConfigureAwait(false);
        if (transcription == null)
            throw new InvalidOperationException($"Azure Speech returned an empty payload for transcription '{transcriptionId}'.");
        transcription.Id ??= transcriptionId;
        return transcription;
    }

    public async Task<BatchTranscriptionFilesResponse> GetFilesAsync(string transcriptionId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"{TranscriptionsPath}/{transcriptionId}/files", cancellationToken).ConfigureAwait(false);
        await ThrowIfFailedAsync(response, cancellationToken).ConfigureAwait(false);
        var files = await DeserializeAsync<BatchTranscriptionFilesResponse>(response, cancellationToken).ConfigureAwait(false);
        if (files == null)
            throw new InvalidOperationException($"Azure Speech did not return any files for transcription '{transcriptionId}'.");
        return files;
    }

    public async Task<string> DownloadContentAsync(Uri contentUri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(contentUri, cancellationToken).ConfigureAwait(false);
        await ThrowIfFailedAsync(response, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private void ConfigureHttpClient()
    {
        if (string.IsNullOrWhiteSpace(_options.AzureSpeechKey))
            throw new InvalidOperationException("Azure Speech key is missing.");

        _httpClient.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _options.AzureSpeechKey);
        _httpClient.Timeout = Timeout.InfiniteTimeSpan;

        var endpoint = ResolveEndpoint();
        if (_httpClient.BaseAddress == null || _httpClient.BaseAddress != endpoint)
        {
            _httpClient.BaseAddress = endpoint;
        }
    }

    private Uri ResolveEndpoint()
    {
        if (!string.IsNullOrWhiteSpace(_options.AzureSpeechEndpoint))
            return EnsureTrailingSlash(new Uri(_options.AzureSpeechEndpoint, UriKind.Absolute));

        if (string.IsNullOrWhiteSpace(_options.AzureSpeechRegion))
            throw new InvalidOperationException("Either AzureSpeechEndpoint or AzureSpeechRegion must be configured.");

        return new Uri($"https://{_options.AzureSpeechRegion}.api.cognitive.microsoft.com/", UriKind.Absolute);
    }

    private static Uri EnsureTrailingSlash(Uri endpoint)
    {
        return endpoint.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? endpoint
            : new Uri(endpoint.AbsoluteUri + "/", UriKind.Absolute);
    }

    private static string? GetIdFromLocation(Uri? location)
    {
        if (location == null) return null;
        var segments = location.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.LastOrDefault();
    }

    private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content == null) return default;
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        if (stream == null) return default;

        if (stream.CanSeek && stream.Length == 0)
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            return default;
        }

        await using (stream)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ThrowIfFailedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var content = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogError("Azure Speech request failed. Status: {StatusCode}, Body: {Body}", (int)response.StatusCode, content);
        throw new HttpRequestException($"Azure Speech request failed with status {(int)response.StatusCode}: {content}");
    }
}
