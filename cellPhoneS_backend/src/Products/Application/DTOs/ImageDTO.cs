namespace cellPhoneS_backend.DTOs;

public record ImageDTO(string? OriginUrl, string? MimeType, string? Name, string? Alt, string? BlobUrl = null, long? Id = null)
{
    public ImageDTO(string? originUrl, string? mimeType, string? name, string? alt) : this(originUrl, mimeType, name, alt, null, null) { }
}
