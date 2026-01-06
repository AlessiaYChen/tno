using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TNO.Services.AutoClipper.Azure.Models;

public class BatchTranscription
{
    public string? Id { get; set; }

    public string? Status { get; set; }

    public string? Locale { get; set; }

    public string? DisplayName { get; set; }

    public DateTimeOffset? CreatedDateTime { get; set; }

    public DateTimeOffset? LastActionDateTime { get; set; }

    public BatchTranscriptionProperties? Properties { get; set; }

    public BatchTranscriptionLinks? Links { get; set; }
}

public class BatchTranscriptionProperties
{
    public bool WordLevelTimestampsEnabled { get; set; }

    public bool DiarizationEnabled { get; set; }

    public string? ProfanityFilterMode { get; set; }

    public string? PunctuationMode { get; set; }

    public BatchTranscriptionError? Error { get; set; }
}

public class BatchTranscriptionError
{
    public string? Code { get; set; }

    public string? Message { get; set; }
}

public class BatchTranscriptionLinks
{
    public Uri? Files { get; set; }
}

public class BatchTranscriptionCreateRequest
{
    public string DisplayName { get; set; } = string.Empty;

    public string Locale { get; set; } = string.Empty;

    public IEnumerable<string> ContentUrls { get; set; } = Array.Empty<string>();

    public BatchTranscriptionCreateProperties Properties { get; set; } = new();
}

public class BatchTranscriptionCreateProperties
{
    public bool WordLevelTimestampsEnabled { get; set; }

    public bool DiarizationEnabled { get; set; }

    public string? ProfanityFilterMode { get; set; }

    public string? PunctuationMode { get; set; }

    public BatchTranscriptionDiarization? Diarization { get; set; }
}

public class BatchTranscriptionDiarization
{
    public BatchTranscriptionDiarizationSpeakers? Speakers { get; set; }

    public string? Mode { get; set; }
}

public class BatchTranscriptionDiarizationSpeakers
{
    public int? MinCount { get; set; }

    public int? MaxCount { get; set; }
}

public class BatchTranscriptionFilesResponse
{
    public List<BatchTranscriptionFile> Values { get; set; } = new();
}

public class BatchTranscriptionFile
{
    public string? Name { get; set; }

    public string? Kind { get; set; }

    public Uri? ContentUrl { get; set; }

    public BatchTranscriptionFileLinks? Links { get; set; }

    public Dictionary<string, JsonElement>? Properties { get; set; }
}

public class BatchTranscriptionFileLinks
{
    public Uri? ContentUrl { get; set; }
}
