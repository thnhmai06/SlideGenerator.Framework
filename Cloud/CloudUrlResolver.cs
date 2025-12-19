using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SlideGenerator.Framework.Cloud.Exceptions;

namespace SlideGenerator.Framework.Cloud;

/// <summary>
///     Provides URL resolution for cloud storage services.
/// </summary>
public static partial class CloudUrlResolver
{
    private static readonly Regex GoogleDriveFileIdPattern = GoogleDriveFileIdRegex();
    private static readonly Regex GoogleDriveFolderFileIdPattern = GoogleDriveFolderFileIdRegex();
    private static readonly Regex GooglePhotosUrlPattern = GooglePhotosUrlRegex();

    /// <summary>
    ///     Resolves a cloud storage URL to a direct download link.
    /// </summary>
    /// <param name="url">The original URL (Google Drive, OneDrive, or Google Photos).</param>
    /// <param name="httpClient">HTTP client for fetching page content when needed.</param>
    /// <returns>A direct download URI.</returns>
    /// <exception cref="CannotExtractUrlException">Thrown when the URL cannot be resolved.</exception>
    /// <exception cref="UriFormatException">Thrown when the URL is not a valid URI.</exception>
    public static async Task<Uri> ResolveLinkAsync(string url, HttpClient? httpClient = null)
        => await ResolveLinkAsync(new Uri(url), httpClient);

    /// <summary>
    ///     Resolves a cloud storage URI to a direct download link.
    /// </summary>
    /// <param name="uri">The original URI (Google Drive, OneDrive, or Google Photos).</param>
    /// <param name="httpClient">HTTP client for fetching page content when needed.</param>
    /// <returns>A direct download URI.</returns>
    /// <exception cref="CannotExtractUrlException">Thrown when the URI cannot be resolved.</exception>
    public static async Task<Uri> ResolveLinkAsync(Uri uri, HttpClient? httpClient = null)
    {
        httpClient ??= new HttpClient();

        // Google Drive link
        if (uri.Host.EndsWith("drive.google.com", StringComparison.OrdinalIgnoreCase))
            return await ResolveGoogleDriveAsync(uri, httpClient);

        // OneDrive link
        if (uri.Host.EndsWith("1drv.ms", StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith("onedrive.live.com", StringComparison.OrdinalIgnoreCase))
            return ResolveOneDrive(uri);

        // Google Photos link
        if (uri.Host.EndsWith("photos.app.goo.gl", StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith("photos.google.com", StringComparison.OrdinalIgnoreCase))
            return await ResolveGooglePhotosAsync(uri, httpClient);

        // Direct link
        return uri;
    }

    /// <summary>
    ///     Checks if the URI is from a supported cloud service.
    /// </summary>
    /// <param name="uri">The URI to check.</param>
    /// <returns><see langword="true"/> if the URI is from a supported cloud service, otherwise <see langword="false"/>.</returns>
    public static bool IsCloudUrlSupported(Uri uri)
    {
        return uri.Host.EndsWith("drive.google.com", StringComparison.OrdinalIgnoreCase)
               || uri.Host.EndsWith("1drv.ms", StringComparison.OrdinalIgnoreCase)
               || uri.Host.EndsWith("onedrive.live.com", StringComparison.OrdinalIgnoreCase)
               || uri.Host.EndsWith("photos.app.goo.gl", StringComparison.OrdinalIgnoreCase)
               || uri.Host.EndsWith("photos.google.com", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<Uri> ResolveGoogleDriveAsync(Uri uri, HttpClient httpClient)
    {
        string? fileId = null;
        var url = uri.ToString();

        // File
        if (uri.AbsolutePath.Contains("/file/d/"))
        {
            var match = GoogleDriveFileIdPattern.Match(url);
            if (match.Success) fileId = match.Groups[1].Value;
        }
        else if (uri.Query.Contains("id="))
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            fileId = query["id"];
        }
        // Folder
        else if (uri.AbsolutePath.Contains("/folders/"))
        {
            var html = await httpClient.GetStringAsync(url);
            var match = GoogleDriveFolderFileIdPattern.Match(html);
            if (match.Success) fileId = match.Groups[1].Value; // first file of folder
        }

        return string.IsNullOrEmpty(fileId)
            ? throw new CannotExtractUrlException("Google Drive", url)
            : new Uri($"https://drive.google.com/uc?export=download&id={fileId}");
    }

    private static Uri ResolveOneDrive(Uri uri)
    {
        var url = uri.ToString();
        var shareToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(url))
            .TrimEnd('=');
        return new Uri($"https://api.onedrive.com/v1.0/shares/u!{shareToken}/root/content");
    }

    private static async Task<Uri> ResolveGooglePhotosAsync(Uri uri, HttpClient httpClient)
    {
        var url = uri.ToString();
        var html = await httpClient.GetStringAsync(url);
        var match = GooglePhotosUrlPattern.Match(html);
        return match.Success
            ? new Uri(match.Value)
            : throw new CannotExtractUrlException("Google Photos", url);
    }

    [GeneratedRegex(@"/file/d/([^/]+)", RegexOptions.Compiled)]
    private static partial Regex GoogleDriveFileIdRegex();

    [GeneratedRegex(@"/file/d/([^\\]+)", RegexOptions.Compiled)]
    private static partial Regex GoogleDriveFolderFileIdRegex();

    [GeneratedRegex(@"https://lh3\.googleusercontent\.com/[^""]*", RegexOptions.Compiled)]
    private static partial Regex GooglePhotosUrlRegex();
}