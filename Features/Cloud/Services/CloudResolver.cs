using SlideGenerator.Framework.Cloud.Entities;

namespace SlideGenerator.Framework.Cloud.Services;

/// <summary>
///     Provides a singleton service for resolving URIs using a set of supported cloud providers.
/// </summary>
/// <remarks>
///     <see cref="CloudResolver" /> maintains a collection of cloud providers and delegates URI resolution to the
///     appropriate provider based on the input URI. The set of providers can be accessed and modified through the
///     Providers
///     property. This class is thread-safe for read operations, but modifications to the Providers collection should be
///     synchronized if accessed concurrently.
/// </remarks>
public sealed class CloudResolver
{
    private static readonly Lazy<CloudResolver> LazyInstance = new(() => new CloudResolver());

    private CloudResolver()
    {
    }

    public static CloudResolver Instance => LazyInstance.Value;

    /// <summary>
    ///     Gets the set of cloud providers associated with this instance.
    /// </summary>
    /// <remarks>
    ///     The set may be empty if no providers have been added. Modifying this collection directly
    ///     affects the providers associated with the instance.
    /// </remarks>
    public HashSet<CloudProvider> Providers { get; } =
    [
        Entities.GoogleDriveProvider.Instance,
        Entities.GooglePhotosProvider.Instance,
        OneDriveProvider.Instance
    ];

    /// <summary>
    ///     Resolves the given URI using the appropriate cloud provider.
    /// </summary>
    /// <param name="uri">The URI to resolve.</param>
    /// <param name="httpClient">An optional HttpClient to use for network requests.</param>
    /// <returns>The resolved or original URI, or null if resolution fails.</returns>
    public async Task<Uri?> ResolveLinkAsync(Uri uri, HttpClient? httpClient = null)
    {
        foreach (var resolver in Providers.Where(resolver => resolver.IsUriSupported(uri)))
        {
            httpClient ??= new HttpClient();
            return await resolver.ResolveUriAsync(uri, httpClient);
        }

        return uri;
    }

    /// <summary>
    ///     Checks if the URI is from a supported cloud service.
    /// </summary>
    /// <param name="uri">The URI to check.</param>
    /// <returns><see langword="true" /> if the URI is from a supported cloud service, otherwise <see langword="false" />.</returns>
    public bool IsUriSupported(Uri uri)
    {
        return Providers.Any(resolver => resolver.IsUriSupported(uri));
    }
}