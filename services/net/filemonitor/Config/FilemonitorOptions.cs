using TNO.Services.Config;

namespace TNO.Services.FileMonitor.Config;

/// <summary>
/// FileMonitorOptions class, configuration options for syndication
/// </summary>
public class FileMonitorOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";
    #endregion
}
