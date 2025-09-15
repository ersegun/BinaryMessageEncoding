using BinaryMessageEncodingAPI.Exceptions;
using BinaryMessageEncodingAPI.Models;
using BinaryMessageEncodingAPI.Services;
using BinaryMessageEncodingAPI.Services.Validation;
using FluentValidation;
using Microsoft.Extensions.Options;
using System.Text;

namespace BinaryMessageEncodingAPI.nUnitTest
{
    [TestFixture]
    public class MessageServiceTests
    {
        private IValidator<Message> _validator = null!;
        private IOptions<MessageOptions> _options = null!;

        [SetUp]
        public void Setup()
        {
            _options = Options.Create(new MessageOptions
            {
                MaxPayloadBytes = 1024,
                MaxHeaders = 2,
                MaxHeaderKeyBytes = 8,
                MaxHeaderValueBytes = 16
            });
            _validator = new MessageValidator(_options);
        }

        [Test]
        public void Encode_Decode_Success()
        {
            var payloadBytes = Encoding.ASCII.GetBytes("hello");

            var originalMessage = new Message
            {
                Headers = new Dictionary<string, string> { { "Type", "json" } },
                Payload = payloadBytes
            };

            var codec = new MessageCodec(_validator, _options);

            byte[] encodedData = codec.Encode(originalMessage);
            Message decodedMessage = codec.Decode(encodedData);

            Assert.That(decodedMessage.Headers["Type"], Is.EqualTo("json"));
            Assert.That(decodedMessage.Payload, Is.EqualTo(payloadBytes));
        }

        [Test]
        public void Decode_Truncated_ThrowsMessageCodecException()
        {
            var message = new Message
            {
                Headers = new Dictionary<string, string> { { "A", "B" } },
                Payload = new byte[10]
            };

            var codec = new MessageCodec(_validator, _options);
            var encoded = codec.Encode(message);

            var truncated = encoded[..^3];
            Assert.That(() => codec.Decode(truncated),
                Throws.TypeOf<MessageCodecException>()
                      .With.InnerException.TypeOf<InvalidDataException>());
        }

        [Test]
        public void HeaderKey_TooLong_ShouldFailValidation()
        {
            var longKey = new string('K', _options.Value.MaxHeaderKeyBytes + 1);
            var message = new Message
            {
                Headers = new Dictionary<string, string> { { longKey, "value" } },
                Payload = Encoding.ASCII.GetBytes("ok")
            };

            var codec = new MessageCodec(_validator, _options);
            Assert.That(() => codec.Encode(message), Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void HeaderValue_TooLong_ShouldFailValidation()
        {
            var longValue = new string('V', _options.Value.MaxHeaderValueBytes + 1);
            var message = new Message
            {
                Headers = new Dictionary<string, string> { { "Key", longValue } },
                Payload = Encoding.ASCII.GetBytes("ok")
            };

            var codec = new MessageCodec(_validator, _options);
            Assert.That(() => codec.Encode(message), Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void TooManyHeaders_ShouldFailValidation()
        {
            var headers = new Dictionary<string, string>();
            for (int i = 0; i < _options.Value.MaxHeaders + 1; i++)
                headers.Add("H" + i, "val");

            var message = new Message
            {
                Headers = headers,
                Payload = Encoding.ASCII.GetBytes("ok")
            };

            var codec = new MessageCodec(_validator, _options);
            Assert.That(() => codec.Encode(message), Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void Payload_TooLarge_ShouldFailValidation()
        {
            var bigPayload = new byte[_options.Value.MaxPayloadBytes + 1];

            var message = new Message
            {
                Headers = new Dictionary<string, string> { { "A", "B" } },
                Payload = bigPayload
            };

            var codec = new MessageCodec(_validator, _options);
            Assert.That(() => codec.Encode(message), Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void Encode_WithNonAsciiHeader_ShouldThrow()
        {
            var message = new Message
            {
                Headers = new Dictionary<string, string> { { "Tÿpe", "json" } }, // non-ASCII
                Payload = Encoding.ASCII.GetBytes("data")
            };

            var codec = new MessageCodec(_validator, _options);

            Assert.That(() => codec.Encode(message),
                Throws.TypeOf<ValidationException>());
        }

        [Test]
        public void Encode_Decode_MaxHeaderKeyValueSize_Success()
        {
            var maxKey = new string('K', _options.Value.MaxHeaderKeyBytes);
            var maxValue = new string('V', _options.Value.MaxHeaderValueBytes);

            var message = new Message
            {
                Headers = new Dictionary<string, string> { { maxKey, maxValue } },
                Payload = Encoding.ASCII.GetBytes("ok")
            };

            var codec = new MessageCodec(_validator, _options);
            var encoded = codec.Encode(message);
            var decoded = codec.Decode(encoded);

            Assert.That(decoded.Headers[maxKey], Is.EqualTo(maxValue));
        }

        [Test]
        public void Decode_WithHeaderLengthOver1023_ShouldThrow()
        {
            var codec = new MessageCodec(_validator, _options);

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)1); // one header
            writer.Write((ushort)2000); // invalid key length
            writer.Write(new byte[2000]); // dummy bytes
            writer.Write((ushort)3);
            writer.Write(Encoding.ASCII.GetBytes("val"));
            writer.Write(0);

            var data = ms.ToArray();

            Assert.That(() => codec.Decode(data),
                Throws.TypeOf<MessageCodecException>()
                      .With.InnerException.TypeOf<InvalidDataException>());
        }

        [Test]
        public void EncodeDecode_EmptyHeadersAndPayload_Success()
        {
            var message = new Message
            {
                Headers = new Dictionary<string, string>(),
                Payload = Array.Empty<byte>()
            };

            var codec = new MessageCodec(_validator, _options);
            var encoded = codec.Encode(message);
            var decoded = codec.Decode(encoded);

            Assert.That(decoded.Headers, Is.Empty);
            Assert.That(decoded.Payload, Is.Empty);
        }

        [Test]
        public void HeaderKey_TooLong_ShouldFailValidation2()
        {
            var longKey = new string('K', _options.Value.MaxHeaderKeyBytes);
            var message = new Message
            {
                Headers = new Dictionary<string, string> { { longKey, "value" } },
                Payload = Encoding.ASCII.GetBytes("ok")
            };

            var codec = new MessageCodec(_validator, _options);
            Assert.That(() => codec.Encode(message), Throws.TypeOf<ValidationException>());
        }
    }
}
