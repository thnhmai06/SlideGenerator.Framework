# Cloud Module Documentation

[🇻🇳 Vietnamese Version](../vi/cloud-module.md)

## Overview

The Cloud module resolves shareable links from popular cloud storage platforms into direct download URLs. This enables the Framework to access cloud-hosted images without requiring manual downloads or authentication management.

## Supported Platforms

| Platform | Share Link Format | Example |
|----------|------------------|---------|
| **Google Drive** | `https://drive.google.com/file/d/{ID}/view` | Works with direct shares |
| **OneDrive** | `https://onedrive.live.com/...` | Works with shared links |
| **Google Photos** | `https://photos.app.goo.gl/{ID}` | Works with shared albums/photos |

## Architecture

### CloudUrlResolver

Static utility class for resolving cloud links:

```csharp
namespace SlideGenerator.Framework.Features.Cloud.Services;

public static class CloudUrlResolver
{
    /// <summary>
    ///     Resolves a shareable cloud link to a direct download URL.
    /// </summary>
    /// <param name="shareLink">The shareable URL from cloud platform.</param>
    /// <returns>Direct download URL that can be used with HttpClient.</returns>
    public static Task<string> ResolveLinkAsync(string shareLink);
}
```

## Usage

### Basic Resolution

```csharp
using SlideGenerator.Framework.Features.Cloud.Services;

public class ImageDownloader
{
    public async Task<Stream> DownloadFromCloudAsync(string shareLink)
    {
        // Resolve shareable link to direct URL
        var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
        
        // Use HttpClient to download
        using var client = new HttpClient();
        return await client.GetStreamAsync(directUrl);
    }
}
```

### Google Drive Example

```csharp
// Input: https://drive.google.com/file/d/1ABC123DEF456/view
var shareLink = "https://drive.google.com/file/d/1ABC123DEF456/view";
var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
// Output: https://drive.google.com/uc?export=download&id=1ABC123DEF456
```

### OneDrive Example

```csharp
// Input: https://onedrive.live.com/?authkey=...&cid=...&id=...&parId=...
var shareLink = "https://onedrive.live.com/?authkey=...";
var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
// Output: Direct download URL with access token
```

## Implementation Details

### Google Drive Resolution

The resolver extracts the file ID from Google Drive share links and constructs a direct download URL:

```
Input:  https://drive.google.com/file/d/{FILE_ID}/view
Output: https://drive.google.com/uc?export=download&id={FILE_ID}
```

**Features:**
- Supports both `/view` and `/preview` endpoints
- Handles URL parameters and fragments
- No authentication required for public shares

### OneDrive Resolution

OneDrive shares require parsing the embedded access token and constructing a download endpoint:

```
Input:  https://onedrive.live.com/?authkey={AUTHKEY}&cid={CID}&id={ITEM_ID}&parId={PAR_ID}
Output: https://onedrive.live.com/download?resauth={RESAUTH}&authkey={AUTHKEY}&cid={CID}&id={ITEM_ID}
```

### Google Photos Resolution

Google Photos short links are expanded to full resolution image URLs.

## Error Handling

```csharp
try
{
    var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
}
catch (ArgumentException ex)
{
    // Invalid URL format
    // Check: URL is valid, platform is supported
}
catch (HttpRequestException ex)
{
    // Network error or invalid share
    // Check: Link is public, network connectivity
}
```

## Performance

- **Resolution Time**: ~100-500ms per link (depends on platform)
- **Network Calls**: 1 HTTP request to cloud platform
- **Caching**: URLs should be cached client-side if accessing same link multiple times

## Best Practices

1. **Cache Resolved URLs**: Cloud platforms may rate-limit repeated lookups
   ```csharp
   var resolvedCache = new Dictionary<string, string>();
   
   if (!resolvedCache.ContainsKey(shareLink))
   {
       resolvedCache[shareLink] = await CloudUrlResolver.ResolveLinkAsync(shareLink);
   }
   
   var directUrl = resolvedCache[shareLink];
   ```

2. **Validate Share Links**: Check URL format before resolving
   ```csharp
   if (!IsValidCloudUrl(shareLink))
       throw new ArgumentException("Invalid cloud share link");
   
   var directUrl = await CloudUrlResolver.ResolveLinkAsync(shareLink);
   ```

3. **Handle Network Errors**: Cloud resolution may fail temporarily
   ```csharp
   var maxRetries = 3;
   for (int i = 0; i < maxRetries; i++)
   {
       try
       {
           return await CloudUrlResolver.ResolveLinkAsync(shareLink);
       }
       catch (HttpRequestException) when (i < maxRetries - 1)
       {
           await Task.Delay(1000 * (i + 1));
       }
   }
   ```

## Limitations

- ❌ Private/restricted shares require authentication
- ❌ Expired share links will fail
- ❌ Rate limiting may apply for high-volume requests
- ✅ Public shares from supported platforms work out of the box
- ✅ No API keys or authentication tokens needed for public shares

## Thread Safety

`CloudUrlResolver` is thread-safe and can be called from multiple threads concurrently.

---

Next: [Sheet Module](sheet-module.md) | [Overview](overview.md)

