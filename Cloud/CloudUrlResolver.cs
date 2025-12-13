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
    /// <returns>A direct download URL.</returns>
    /// <exception cref="CloudUrlExtractionException">Thrown when the URL cannot be resolved.</exception>
    public static async Task<string> ResolveAsync(string url, HttpClient httpClient)
    {
        // Google Drive link
        if (url.Contains("drive.google.com")) return await ResolveGoogleDriveAsync(url, httpClient);

        // OneDrive link
        if (url.Contains("1drv.ms") || url.Contains("onedrive.live.com")) return ResolveOneDrive(url);

        // Google Photos link
        if (url.Contains("photos.app.goo.gl") || url.Contains("photos.google.com"))
            return await ResolveGooglePhotosAsync(url, httpClient);

        // Direct link - return as-is
        return url;
    }

    /// <summary>
    ///     Checks if the URL is from a supported cloud storage service.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if the URL is from a cloud storage service.</returns>
    public static bool IsCloudStorageUrl(string url)
    {
        return url.Contains("drive.google.com") ||
               url.Contains("1drv.ms") ||
               url.Contains("onedrive.live.com") ||
               url.Contains("photos.app.goo.gl") ||
               url.Contains("photos.google.com");
    }

    private static async Task<string> ResolveGoogleDriveAsync(string url, HttpClient httpClient)
    {
        string? imageId = null;

        if (url.Contains("/file/d/"))
        {
            var match = GoogleDriveFileIdPattern.Match(url);
            if (match.Success) imageId = match.Groups[1].Value;
        }
        else if (url.Contains("id="))
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            imageId = query["id"];
        }
        else if (url.Contains("/folders/"))
        {
            var html = await httpClient.GetStringAsync(url);
            var match = GoogleDriveFolderFileIdPattern.Match(html);
            if (match.Success) imageId = match.Groups[1].Value;
        }

        return string.IsNullOrEmpty(imageId)
            ? throw new CloudUrlExtractionException("Google Drive", url)
            : $"https://drive.google.com/uc?export=download&id={imageId}";
    }

    private static string ResolveOneDrive(string url)
    {
        var shareToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(url))
            .TrimEnd('=');
        return $"https://api.onedrive.com/v1.0/shares/u!{shareToken}/root/content";
    }

    private static async Task<string> ResolveGooglePhotosAsync(string url, HttpClient httpClient)
    {
        var html = await httpClient.GetStringAsync(url);
        var match = GooglePhotosUrlPattern.Match(html);
        return match.Success
            ? match.Value
            : throw new CloudUrlExtractionException("Google Photos", url);
    }

    [GeneratedRegex(@"/file/d/([^/]+)", RegexOptions.Compiled)]
    private static partial Regex GoogleDriveFileIdRegex();

    [GeneratedRegex(@"/file/d/([^\\]+)", RegexOptions.Compiled)]
    private static partial Regex GoogleDriveFolderFileIdRegex();

    [GeneratedRegex(@"https://lh3\.googleusercontent\.com/[^""]*", RegexOptions.Compiled)]
    private static partial Regex GooglePhotosUrlRegex();
}