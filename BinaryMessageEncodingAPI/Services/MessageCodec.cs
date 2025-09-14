using BinaryMessageEncodingAPI.Exceptions;
using BinaryMessageEncodingAPI.Models;
using FluentValidation;
using Microsoft.Extensions.Options;
using System.Text;

namespace BinaryMessageEncodingAPI.Services;

public sealed class MessageCodec : IMessageCodec
{
    private readonly IValidator<Message> _validator;
    private readonly MessageOptions _options;

    public MessageCodec(IValidator<Message> validator, IOptions<MessageOptions> options)
    {
        _validator = validator;
        _options = options.Value;
    }

    public byte[] Encode(Message message)
    {
        _validator.ValidateAndThrow(message);

        try
        {
            using MemoryStream stream = new();
            using BinaryWriter writer = new(stream, Encoding.ASCII, leaveOpen: true);

            // Write header count
            writer.Write((byte)message.Headers.Count);

            // Write headers
            foreach (var header in message.Headers)
            {
                WriteString(writer, header.Key);
                WriteString(writer, header.Value);
            }

            // Write payload size (int32 is fine, max 256 KiB)
            writer.Write(message.Payload.Length);

            // Write payload
            writer.Write(message.Payload);

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            throw new MessageCodecException("Encoding failed.", ex);
        }
    }

    public Message Decode(byte[] data)
    {
        try
        {
            using var stream = new MemoryStream(data, writable: false);
            using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: false);

            // Read header count
            byte headerCount = reader.ReadByte();

            var headers = new Dictionary<string, string>(headerCount);

            // Read header name values.
            for (int i = 0; i < headerCount; i++)
            {
                var name = ReadString(reader);
                var value = ReadString(reader);
                headers[name] = value;
            }

            // Read payload size
            int payloadSize = reader.ReadInt32();
            if (payloadSize < 0 || payloadSize > _options.MaxPayloadBytes)
                throw new InvalidDataException($"Invalid payload size: {payloadSize}");

            // Read payload
            byte[] payload = reader.ReadBytes(payloadSize);

            // Check for truncation
            if (payload.Length != payloadSize)
                throw new InvalidDataException("Truncated payload detected.");

            var message = new Message { Headers = headers, Payload = payload };

            // Run semantic validation (ASCII + size rules)
            _validator.ValidateAndThrow(message);

            return message;
        }
        catch (Exception ex)
        {
            throw new MessageCodecException("Decoding failed.", ex);
        }
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

    private static string ReadString(BinaryReader reader)
    {
        ushort length = reader.ReadUInt16();
        if (length > 1023)
            throw new InvalidDataException($"Header string length {length} exceeds 1023 bytes.");

        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
            throw new InvalidDataException("Unexpected end of stream while reading string.");

        return Encoding.ASCII.GetString(bytes);
    }
}
