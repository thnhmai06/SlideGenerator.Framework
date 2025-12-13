# Cloud URL API

## Resolution
- `CloudUrlResolver.ResolveAsync(url, HttpClient)` converts Google Drive/Photos/OneDrive share links to direct-download URLs when possible.
- `IsCloudStorageUrl(url)` detects supported providers.

## Exceptions
- Throws `CloudUrlExtractionException` when a direct link cannot be extracted.
