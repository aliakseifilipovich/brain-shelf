using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BrainShelf.Services;

/// <summary>
/// Service for extracting metadata from web pages
/// Uses HtmlAgilityPack to parse HTML and extract meta tags, Open Graph tags, and Twitter Card metadata
/// </summary>
public class MetadataExtractionService : IMetadataExtractionService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MetadataExtractionService> _logger;
    private const int TimeoutSeconds = 10;

    public MetadataExtractionService(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<MetadataExtractionService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Metadata?> ExtractMetadataAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                _logger.LogWarning("Invalid URL format: {Url}", url);
                return null;
            }

            // Fetch HTML content with timeout
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));

            var response = await httpClient.GetAsync(uri, cts.Token);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(cts.Token);

            // Parse HTML
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // Extract metadata
            var metadata = new Metadata
            {
                Title = ExtractTitle(htmlDoc, uri),
                Description = ExtractDescription(htmlDoc),
                Keywords = ExtractKeywords(htmlDoc),
                ImageUrl = ExtractImageUrl(htmlDoc, uri),
                FaviconUrl = ExtractFaviconUrl(htmlDoc, uri),
                Author = ExtractAuthor(htmlDoc),
                SiteName = ExtractSiteName(htmlDoc)
            };

            _logger.LogInformation("Successfully extracted metadata from {Url}", url);
            return metadata;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Metadata extraction timed out for URL: {Url}", url);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP request failed for URL: {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error extracting metadata from URL: {Url}", url);
            return null;
        }
    }

    public async Task<bool> ExtractAndSaveMetadataAsync(Guid entryId, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = await ExtractMetadataAsync(url, cancellationToken);
            if (metadata is null)
            {
                return false;
            }

            // Check if entry exists
            var entry = await _context.Entries
                .Include(e => e.Metadata)
                .FirstOrDefaultAsync(e => e.Id == entryId, cancellationToken);

            if (entry is null)
            {
                _logger.LogWarning("Entry not found: {EntryId}", entryId);
                return false;
            }

            // Update or create metadata
            if (entry.Metadata is not null)
            {
                // Update existing metadata
                entry.Metadata.Title = metadata.Title;
                entry.Metadata.Description = metadata.Description;
                entry.Metadata.Keywords = metadata.Keywords;
                entry.Metadata.ImageUrl = metadata.ImageUrl;
                entry.Metadata.FaviconUrl = metadata.FaviconUrl;
                entry.Metadata.Author = metadata.Author;
                entry.Metadata.SiteName = metadata.SiteName;
                entry.Metadata.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new metadata
                metadata.EntryId = entryId;
                _context.Metadata.Add(metadata);
                entry.Metadata = metadata;
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully saved metadata for entry {EntryId}", entryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving metadata for entry {EntryId}", entryId);
            return false;
        }
    }

    private string? ExtractTitle(HtmlDocument doc, Uri uri)
    {
        // Try Open Graph title first
        var ogTitle = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrWhiteSpace(ogTitle))
        {
            return ogTitle.Trim();
        }

        // Try Twitter Card title
        var twitterTitle = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrWhiteSpace(twitterTitle))
        {
            return twitterTitle.Trim();
        }

        // Fallback to HTML title tag
        var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;
        if (!string.IsNullOrWhiteSpace(title))
        {
            return HtmlEntity.DeEntitize(title.Trim());
        }

        // Last resort: use URL
        return uri.Host;
    }

    private string? ExtractDescription(HtmlDocument doc)
    {
        // Try Open Graph description first
        var ogDescription = doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrWhiteSpace(ogDescription))
        {
            return ogDescription.Trim();
        }

        // Try Twitter Card description
        var twitterDescription = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrWhiteSpace(twitterDescription))
        {
            return twitterDescription.Trim();
        }

        // Fallback to meta description
        var description = doc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", null);
        return !string.IsNullOrWhiteSpace(description) ? description.Trim() : null;
    }

    private string? ExtractKeywords(HtmlDocument doc)
    {
        var keywords = doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']")?.GetAttributeValue("content", null);
        return !string.IsNullOrWhiteSpace(keywords) ? keywords.Trim() : null;
    }

    private string? ExtractImageUrl(HtmlDocument doc, Uri baseUri)
    {
        // Try Open Graph image first
        var ogImage = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrWhiteSpace(ogImage))
        {
            return ResolveUrl(ogImage.Trim(), baseUri);
        }

        // Try Twitter Card image
        var twitterImage = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']")?.GetAttributeValue("content", null);
        if (!string.IsNullOrWhiteSpace(twitterImage))
        {
            return ResolveUrl(twitterImage.Trim(), baseUri);
        }

        return null;
    }

    private string? ExtractFaviconUrl(HtmlDocument doc, Uri baseUri)
    {
        // Try various favicon link tags
        var faviconSelectors = new[]
        {
            "//link[@rel='icon']",
            "//link[@rel='shortcut icon']",
            "//link[@rel='apple-touch-icon']"
        };

        foreach (var selector in faviconSelectors)
        {
            var faviconNode = doc.DocumentNode.SelectSingleNode(selector);
            var href = faviconNode?.GetAttributeValue("href", null);
            if (!string.IsNullOrWhiteSpace(href))
            {
                return ResolveUrl(href.Trim(), baseUri);
            }
        }

        // Fallback to default favicon location
        return $"{baseUri.Scheme}://{baseUri.Host}/favicon.ico";
    }

    private string? ExtractAuthor(HtmlDocument doc)
    {
        // Try various author meta tags
        var authorSelectors = new[]
        {
            "//meta[@name='author']",
            "//meta[@property='article:author']",
            "//meta[@name='twitter:creator']"
        };

        foreach (var selector in authorSelectors)
        {
            var author = doc.DocumentNode.SelectSingleNode(selector)?.GetAttributeValue("content", null);
            if (!string.IsNullOrWhiteSpace(author))
            {
                return author.Trim();
            }
        }

        return null;
    }

    private string? ExtractSiteName(HtmlDocument doc)
    {
        var siteName = doc.DocumentNode.SelectSingleNode("//meta[@property='og:site_name']")?.GetAttributeValue("content", null);
        return !string.IsNullOrWhiteSpace(siteName) ? siteName.Trim() : null;
    }

    private string? ResolveUrl(string url, Uri baseUri)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // If already absolute URL, return as is
        if (Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return url;
        }

        // Try to resolve relative URL
        if (Uri.TryCreate(baseUri, url, out var resolvedUri))
        {
            return resolvedUri.ToString();
        }

        return null;
    }
}
