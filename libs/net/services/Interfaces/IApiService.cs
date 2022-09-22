using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;

namespace TNO.Services;

/// <summary>
/// IApiService interface, provides a way to interact with the API.
/// </summary>
public interface IApiService
{
    #region Lookups
    /// <summary>
    /// Make an AJAX request to the api to get the lookups.
    /// </summary>
    /// <returns></returns>
    public Task<LookupModel?> GetLookupsAsync();
    #endregion

    #region Sources
    /// <summary>
    /// Make an AJAX request to the api to fetch all sources.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<SourceModel>> GetSourcesAsync();

    /// <summary>
    /// Make an AJAX request to the api to fetch the sources for the specified 'code'.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Task<SourceModel?> GetSourceForCodeAsync(string code);
    #endregion

    #region Connections
    /// <summary>
    /// Make an AJAX request to the api to get the connection.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<ConnectionModel?> GetConnectionAsync(int id);
    #endregion

    #region Ingests
    /// <summary>
    /// Make an AJAX request to the api to fetch the ingest for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<IngestModel?> GetIngestAsync(int id);

    /// <summary>
    /// Make an AJAX request to the api to fetch all ingests.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<IngestModel>> GetIngestsAsync();

    /// <summary>
    /// Make an AJAX request to the api to fetch ingests for the specified media type.
    /// </summary>
    /// <param name="mediaType"></param>
    /// <returns></returns>
    public Task<IEnumerable<IngestModel>> GetIngestsForMediaTypeAsync(string mediaType);

    /// <summary>
    /// Make an AJAX request to the api to fetch the ingest for the specified 'topic'.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public Task<IEnumerable<IngestModel>> GetIngestsForTopicAsync(string topic);
    #endregion

    #region Contents
    /// <summary>
    /// Make an AJAX request to the api to update the content for the specified ContentModel.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public Task<ContentModel?> UpdateContentAsync(ContentModel content);

    /// <summary>
    /// Make an AJAX request to the api to update the ingest.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public Task<IngestModel?> UpdateIngestAsync(IngestModel ingest);

    /// <summary>
    /// Make an AJAX request to the api to find the content reference for the specified key.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public Task<ContentReferenceModel?> FindContentReferenceAsync(string source, string uid);

    /// <summary>
    /// Make an AJAX request to the api to add the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public Task<ContentReferenceModel?> AddContentReferenceAsync(ContentReferenceModel contentReference);

    /// <summary>
    /// Make an AJAX request to the api to update the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel contentReference);

    /// <summary>
    /// Make an AJAX request to the api to add the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public Task<ContentModel?> AddContentAsync(ContentModel content);

    /// <summary>
    /// Make an AJAX request to the api to upload the file and link to specified content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="version"></param>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<ContentModel?> UploadFileAsync(long contentId, long version, Stream file, string fileName);

    /// <summary>
    /// Make an AJAX request to the api to find the specified content.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    Task<ContentModel?> FindContentByUidAsync(string uid, string? source);

    /// <summary>
    /// Make an AJAX request to the api to get the specified content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ContentModel?> FindContentByIdAsync(long id);
    #endregion
}
