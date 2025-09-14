using FluentValidation;

namespace BinaryMessageEncodingAPI.Models;

// record: Value-based equality: two records with the same property values are considered equal.
public sealed record DecodeMessageRequest(string? Base64, byte[]? Bytes)
{
    public byte[] AsBytes()
    {
        if (Bytes is { Length: > 0 })
            return Bytes;

        if (!string.IsNullOrWhiteSpace(Base64))
            return Convert.FromBase64String(Base64);

        throw new ValidationException("Either 'Base64' or 'Bytes' must be provided.");
    }
}
