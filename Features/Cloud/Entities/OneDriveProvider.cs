using System.Text;

namespace SlideGenerator.Framework.Cloud;

/// <summary>
///     Provides access to OneDrive as a cloud provider, enabling resolution and support checks for OneDrive URLs.
/// </summary>
/// <remarks>
///     This class implements the singleton pattern. Use the <see cref="Instance" /> property to access the
///     shared instance. <see cref="OneDriveProvider" /> supports URLs from both 1drv.ms and onedrive.live.com
///     domains.
/// </remarks>
public sealed class OneDriveProvider : CloudProvider
{
    private static readonly Lazy<OneDriveProvider> LazyInstance = new(() => new OneDriveProvider());

    private OneDriveProvider()
    {
    }

    public static OneDriveProvider Instance => LazyInstance.Value;

    internal override Task<Uri?> ResolveUriAsync(Uri supportedUri, HttpClient httpClient)
    {
        var url = supportedUri.ToString();
        var base64Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(url));
        var encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');
        return Task.FromResult(new Uri($"https://api.onedrive.com/v1.0/shares/{encodedUrl}/root/content"))!;
    }

    internal override bool IsUriSupported(Uri uri)
    {
        return uri.Host.EndsWith("1drv.ms", StringComparison.OrdinalIgnoreCase)
               || uri.Host.EndsWith("onedrive.live.com", StringComparison.OrdinalIgnoreCase);
    }
}