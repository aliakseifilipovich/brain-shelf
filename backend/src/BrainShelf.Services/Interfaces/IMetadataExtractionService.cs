using BrainShelf.Core.Entities;

namespace BrainShelf.Services.Interfaces;

/// <summary>
/// Service for extracting metadata from URLs
/// Handles HTML parsing to extract page titles, descriptions, keywords, and preview images
/// </summary>
public interface IMetadataExtractionService
{
    /// <summary>
    /// Extracts metadata from the specified URL asynchronously
    /// Parses HTML to extract meta tags, Open Graph tags, and Twitter Card metadata
    /// </summary>
    /// <param name="url">The URL to extract metadata from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted metadata or null if extraction fails</returns>
    Task<Metadata?> ExtractMetadataAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts and saves metadata for an entry asynchronously
    /// Creates or updates the Metadata entity associated with the entry
    /// </summary>
    /// <param name="entryId">The ID of the entry to extract metadata for</param>
    /// <param name="url">The URL to extract metadata from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if metadata was successfully extracted and saved</returns>
    Task<bool> ExtractAndSaveMetadataAsync(Guid entryId, string url, CancellationToken cancellationToken = default);
}
