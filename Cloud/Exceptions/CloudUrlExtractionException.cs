namespace SlideGenerator.Framework.Cloud.Exceptions;

/// <summary>
///     Exception thrown when unable to extract a direct download URL from cloud storage services.
/// </summary>
public class CloudUrlExtractionException(string serviceName, string originalUrl)
    : Exception($"Cannot extract direct download URL from {serviceName}.")
{
    public string ServiceName { get; } = serviceName;
    public string OriginalUrl { get; } = originalUrl;
}