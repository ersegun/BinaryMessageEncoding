using BinaryMessageEncodingAPI.Models;
using BinaryMessageEncodingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BinaryMessageEncodingAPI.Controllers;

[ApiController]
[Route("api/v1/messages")]
public sealed class MessageController : ControllerBase
{
    private readonly IMessageCodec _messageCodec;
    private readonly ILogger<MessageController> _logger;

    public MessageController(IMessageCodec messageCodec, ILogger<MessageController> logger)
    {
        _messageCodec = messageCodec;
        _logger = logger;
    }

    [HttpPost("encode")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<string> Encode([FromBody] Message message)
    {
        try
        {
            var bytes = _messageCodec.Encode(message);

            _logger.LogInformation("Message encoded successfully with {HeaderCount} headers and payload size {PayloadSize} bytes",
                message.Headers?.Count ?? 0, message.PayloadBase64?.Length ?? 0);

            return Ok(Convert.ToBase64String(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw; // Middleware will convert to ProblemDetails
        }
    }

    [HttpPost("decode")]
    [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<Message> Decode([FromBody] DecodeMessageRequest payload)
    {
        try
        {
            var data = payload.AsBytes();
            var msg = _messageCodec.Decode(data);

            _logger.LogInformation("Message decoded successfully with {HeaderCount} headers and payload size {PayloadSize} bytes",
                msg.Headers.Count, msg.Payload.Length);

            return Ok(msg); // Already serializes with PayloadBase64
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

}
