namespace BinaryMessageEncodingAPI.Services;

public sealed class MessageOptions
{
    public int MaxPayloadBytes { get; set; } = 262144;

    public int MaxHeaders { get; set; } = 63;

    public int MaxHeaderKeyBytes { get; set; } = 1023;

    public int MaxHeaderValueBytes { get; set; } = 1023;
}
