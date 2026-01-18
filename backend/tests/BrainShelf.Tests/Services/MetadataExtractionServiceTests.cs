using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace BrainShelf.Tests.Services;

[TestFixture]
public class MetadataExtractionServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<ILogger<MetadataExtractionService>> _loggerMock = null!;
    private MetadataExtractionService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<MetadataExtractionService>>();
        _service = new MetadataExtractionService(_context, _httpClientFactoryMock.Object, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task ExtractMetadataAsync_WithValidHtml_ExtractsAllMetadata()
    {
        // Arrange
        var html = @"
            <html>
                <head>
                    <title>Test Page Title</title>
                    <meta name=""description"" content=""Test description"">
                    <meta name=""keywords"" content=""test, page, keywords"">
                    <meta property=""og:title"" content=""OG Title"">
                    <meta property=""og:description"" content=""OG Description"">
                    <meta property=""og:image"" content=""https://example.com/image.jpg"">
                    <meta property=""og:site_name"" content=""Test Site"">
                    <meta name=""author"" content=""Test Author"">
                    <link rel=""icon"" href=""/favicon.ico"">
                </head>
            </html>";

        SetupHttpClient(html, HttpStatusCode.OK);

        // Act
        var result = await _service.ExtractMetadataAsync("https://example.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("OG Title")); // Prefers Open Graph
        Assert.That(result.Description, Is.EqualTo("OG Description"));
        Assert.That(result.Keywords, Is.EqualTo("test, page, keywords"));
        Assert.That(result.ImageUrl, Is.EqualTo("https://example.com/image.jpg"));
        Assert.That(result.FaviconUrl, Is.EqualTo("https://example.com/favicon.ico"));
        Assert.That(result.Author, Is.EqualTo("Test Author"));
        Assert.That(result.SiteName, Is.EqualTo("Test Site"));
    }

    [Test]
    public async Task ExtractMetadataAsync_WithMinimalHtml_ExtractsBasicInfo()
    {
        // Arrange
        var html = @"
            <html>
                <head>
                    <title>Simple Page</title>
                </head>
            </html>";

        SetupHttpClient(html, HttpStatusCode.OK);

        // Act
        var result = await _service.ExtractMetadataAsync("https://example.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Simple Page"));
        Assert.That(result.FaviconUrl, Is.EqualTo("https://example.com/favicon.ico")); // Default
    }

    [Test]
    public async Task ExtractMetadataAsync_WithTwitterCardTags_ExtractsTwitterMetadata()
    {
        // Arrange
        var html = @"
            <html>
                <head>
                    <title>Page Title</title>
                    <meta name=""twitter:title"" content=""Twitter Title"">
                    <meta name=""twitter:description"" content=""Twitter Description"">
                    <meta name=""twitter:image"" content=""https://example.com/twitter-image.jpg"">
                    <meta name=""twitter:creator"" content=""@twitteruser"">
                </head>
            </html>";

        SetupHttpClient(html, HttpStatusCode.OK);

        // Act
        var result = await _service.ExtractMetadataAsync("https://example.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Twitter Title"));
        Assert.That(result.Description, Is.EqualTo("Twitter Description"));
        Assert.That(result.ImageUrl, Is.EqualTo("https://example.com/twitter-image.jpg"));
        Assert.That(result.Author, Is.EqualTo("@twitteruser"));
    }

    [Test]
    public async Task ExtractMetadataAsync_WithRelativeImageUrl_ResolvesToAbsoluteUrl()
    {
        // Arrange
        var html = @"
            <html>
                <head>
                    <title>Page</title>
                    <meta property=""og:image"" content=""/images/preview.jpg"">
                </head>
            </html>";

        SetupHttpClient(html, HttpStatusCode.OK);

        // Act
        var result = await _service.ExtractMetadataAsync("https://example.com/page");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ImageUrl, Is.EqualTo("https://example.com/images/preview.jpg"));
    }

    [Test]
    public async Task ExtractMetadataAsync_WithInvalidUrl_ReturnsNull()
    {
        // Act
        var result = await _service.ExtractMetadataAsync("not-a-valid-url");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExtractMetadataAsync_WithFtpUrl_ReturnsNull()
    {
        // Act
        var result = await _service.ExtractMetadataAsync("ftp://example.com");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExtractMetadataAsync_WithHttpError_ReturnsNull()
    {
        // Arrange
        SetupHttpClient("", HttpStatusCode.NotFound);

        // Act
        var result = await _service.ExtractMetadataAsync("https://example.com");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExtractMetadataAsync_WithNoTitle_UsesDomain()
    {
        // Arrange
        var html = @"<html><head></head></html>";
        SetupHttpClient(html, HttpStatusCode.OK);

        // Act
        var result = await _service.ExtractMetadataAsync("https://example.com/page");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("example.com"));
    }

    [Test]
    public async Task ExtractAndSaveMetadataAsync_WithNewMetadata_CreatesMetadata()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var entry = new Entry
        {
            ProjectId = project.Id,
            Title = "Test Entry",
            Type = EntryType.Link,
            Content = "Content",
            Url = "https://example.com"
        };
        _context.Projects.Add(project);
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        var html = @"
            <html>
                <head>
                    <title>Test Page</title>
                    <meta name=""description"" content=""Test description"">
                </head>
            </html>";
        SetupHttpClient(html, HttpStatusCode.OK);

        // Act
        var result = await _service.ExtractAndSaveMetadataAsync(entry.Id, entry.Url);

        // Assert
        Assert.That(result, Is.True);

        var savedEntry = await _context.Entries
            .Include(e => e.Metadata)
            .FirstAsync(e => e.Id == entry.Id);

        Assert.That(savedEntry.Metadata, Is.Not.Null);
        Assert.That(savedEntry.Metadata.Title, Is.EqualTo("Test Page"));
        Assert.That(savedEntry.Metadata.Description, Is.EqualTo("Test description"));
    }

    [Test]
    public async Task ExtractAndSaveMetadataAsync_WithExistingMetadata_UpdatesMetadata()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var entry = new Entry
        {
            ProjectId = project.Id,
            Title = "Test Entry",
            Type = EntryType.Link,
            Content = "Content",
            Url = "https://example.com"
        };
        var existingMetadata = new Metadata
        {
            EntryId = entry.Id,
            Title = "Old Title",
            Description = "Old Description"
        };
        entry.Metadata = existingMetadata;

        _context.Projects.Add(project);
        _context.Entries.Add(entry);
        _context.Metadata.Add(existingMetadata);
        await _context.SaveChangesAsync();

        var html = @"
            <html>
                <head>
                    <title>New Title</title>
                    <meta name=""description"" content=""New Description"">
                </head>
            </html>";
        SetupHttpClient(html, HttpStatusCode.OK);

        // Act
        var result = await _service.ExtractAndSaveMetadataAsync(entry.Id, entry.Url);

        // Assert
        Assert.That(result, Is.True);

        var savedEntry = await _context.Entries
            .Include(e => e.Metadata)
            .FirstAsync(e => e.Id == entry.Id);

        Assert.That(savedEntry.Metadata, Is.Not.Null);
        Assert.That(savedEntry.Metadata.Id, Is.EqualTo(existingMetadata.Id)); // Same metadata entity
        Assert.That(savedEntry.Metadata.Title, Is.EqualTo("New Title"));
        Assert.That(savedEntry.Metadata.Description, Is.EqualTo("New Description"));
    }

    [Test]
    public async Task ExtractAndSaveMetadataAsync_WithNonExistentEntry_ReturnsFalse()
    {
        // Act
        var result = await _service.ExtractAndSaveMetadataAsync(Guid.NewGuid(), "https://example.com");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExtractAndSaveMetadataAsync_WithExtractionFailure_ReturnsFalse()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var entry = new Entry
        {
            ProjectId = project.Id,
            Title = "Test Entry",
            Type = EntryType.Link,
            Content = "Content",
            Url = "https://example.com"
        };
        _context.Projects.Add(project);
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        SetupHttpClient("", HttpStatusCode.InternalServerError);

        // Act
        var result = await _service.ExtractAndSaveMetadataAsync(entry.Id, entry.Url);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExtractMetadataAsync_WithHtmlEntities_DecodesTitle()
    {
        // Arrange
        var html = @"
            <html>
                <head>
                    <title>Test &amp; Page &quot;Title&quot;</title>
                </head>
            </html>";
        SetupHttpClient(html, HttpStatusCode.OK);

        // Act
        var result = await _service.ExtractMetadataAsync("https://example.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Test & Page \"Title\""));
    }

    private void SetupHttpClient(string htmlContent, HttpStatusCode statusCode)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(htmlContent)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
    }
}
