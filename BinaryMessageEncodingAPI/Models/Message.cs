namespace BinaryMessageEncodingAPI.Models;

public sealed record Message
{
    public Dictionary<string, string> Headers { get; set; }

    public byte[] Payload { get; set; }

    public string PayloadBase64
    {
        get => Convert.ToBase64String(Payload ?? Array.Empty<byte>());
        init => Payload = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : Convert.FromBase64String(value);
    }
}
