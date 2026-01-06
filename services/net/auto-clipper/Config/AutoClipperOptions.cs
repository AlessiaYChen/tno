using System;
using System.IO;
using TNO.Services.Config;

namespace TNO.Services.AutoClipper.Config;

public class AutoClipperOptions : ServiceOptions
{
    public string Topics { get; set; } = "request-clips";
    public string VolumePath { get; set; } = "";
    public bool AcceptOnlyWorkOrders { get; set; } = true;
    public string[] ConvertToAudio { get; set; } = Array.Empty<string>();
    public int? IgnoreContentPublishedBeforeOffset { get; set; } = null;
    public string OldTnoContentTagName { get; set; } = "";
    public string AzureSpeechKey { get; set; } = "";
    public string AzureSpeechRegion { get; set; } = "";
    public string AzureSpeechEndpoint { get; set; } = string.Empty;
    public string DefaultTranscriptLanguage { get; set; } = "en-US";
    public int AzureSpeechMaxRetries { get; set; } = 3;
    public int AzureSpeechRetryDelaySeconds { get; set; } = 5;
    public int AzureSpeechBatchPollIntervalSeconds { get; set; } = 15;
    public int AzureSpeechBatchTimeoutMinutes { get; set; } = 60;
    public int AzureSpeechBatchMaxConcurrentJobs { get; set; } = 2;
    public bool AzureSpeechBatchWordLevelTimestampsEnabled { get; set; } = true;
    public bool AzureSpeechBatchDiarizationEnabled { get; set; } = true;
    public string AzureSpeechBatchProfanityFilterMode { get; set; } = "Masked";
    public string AzureSpeechBatchPunctuationMode { get; set; } = "DictatedAndAutomatic";
    public bool AzureSpeechBatchDeleteInputOnCompletion { get; set; } = true;
    public string AzureBlobConnectionString { get; set; } = string.Empty;
    public string AzureBlobInputContainer { get; set; } = "autoclipper-input";
    public int AzureBlobSasValidityMinutes { get; set; } = 180;


    public string LlmApiUrl { get; set; } = "";
    public string LlmApiKey { get; set; } = "";
    public string LlmModel { get; set; } = "";
    public string LlmDeployment { get; set; } = "";
    public string LlmApiVersion { get; set; } = "2024-07-18";
    public string LlmPrompt { get; set; } = string.Empty;
    public int LlmMaxStories { get; set; } = 5;
    public int LlmPromptCharacterLimit { get; set; } = 6000;
    public double LlmTemperature { get; set; } = 0.1;
    public double LlmBoundaryScoreThreshold { get; set; } = 0.55;

    public string StationConfigPath { get; set; } = Path.Combine("Config", "Stations");
}




