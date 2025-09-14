using BinaryMessageEncodingAPI.Models;

namespace BinaryMessageEncodingAPI.Services;

public interface IMessageCodec
{
    byte[] Encode(Message message);

    Message Decode(byte[] data);
}
