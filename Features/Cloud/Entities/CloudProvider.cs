namespace SlideGenerator.Framework.Cloud.Entities;

/// <summary>
///     Provides an abstraction for cloud service providers that can resolve and validate URIs using custom logic.
/// </summary>
/// <remarks>
///     Implement this class to support provider-specific URI resolution and validation in cloud-based
///     applications. Derived classes should define how URIs are resolved and which URIs are supported, enabling
///     extensibility for different cloud platforms.
/// </remarks>
public abstract class CloudProvider
{
    /// <summary>
    ///     Asynchronously resolves the specified URI, potentially following redirects or applying custom resolution logic.
    /// </summary>
    /// <param name="supportedUri">The URI to resolve. Must be a URI that is supported by this provider.</param>
    /// <param name="httpClient">The HTTP client used to perform network requests during URI resolution. Cannot be null.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the resolved URI, or null if
    ///     resolution fails.
    /// </returns>
    internal abstract Task<Uri?> ResolveUriAsync(Uri supportedUri, HttpClient httpClient);

    /// <summary>
    ///     Determines whether the specified URI is supported by the current implementation.
    /// </summary>
    /// <param name="uri">The URI to evaluate for support. Cannot be null.</param>
    /// <returns><see langword="true" /> if the specified URI is supported; otherwise, <see langword="false" />.</returns>
    internal abstract bool IsUriSupported(Uri uri);
}