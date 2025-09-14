using BinaryMessageEncodingAPI.Models;
using FluentValidation;
using Microsoft.Extensions.Options;
using System.Text;

namespace BinaryMessageEncodingAPI.Services.Validation;

/*
 * AbstractValidator<T> is a generic base class provided by the FluentValidation library in C#.
 * It’s used to define validation rules for a specific model type (T).
 * Instead of writing if checks all over the code, you declare all rules in one place.
 * It makes validation declarative, testable, and reusable.
 */

// MessageValidator keeps semantic rules by using FluentValidation library.
public sealed class MessageValidator : AbstractValidator<Message>
{
    public MessageValidator(IOptions<MessageOptions> options)
    {
        var opt = options.Value;

        RuleFor(m => m.Payload)
            .NotNull().WithMessage("Payload is required.")
            .Must(p => p!.Length <= opt.MaxPayloadBytes)
            .WithMessage(m => $"Payload size exceeds {opt.MaxPayloadBytes} bytes.");

        RuleFor(m => m.Headers)
            .NotNull().WithMessage("Headers are required.")
            .Must(h => h!.Count <= opt.MaxHeaders)
            .WithMessage(m => $"Header count exceeds {opt.MaxHeaders}.");

        RuleForEach(m => m.Headers!).ChildRules(dict =>
        {
            dict.RuleFor(kv => kv.Key)
                .NotEmpty()
                .Must(k => IsAscii(k))
                .WithMessage("Header key must contain only ASCII characters.")
                .Must(k => Encoding.ASCII.GetByteCount(k) <= opt.MaxHeaderKeyBytes)
                .WithMessage(m => $"Header key too long (>{opt.MaxHeaderKeyBytes} bytes ASCII).");

            dict.RuleFor(kv => kv.Value)
                .NotNull()
                .Must(v => IsAscii(v!))
                .WithMessage("Header value must contain only ASCII characters.")
                .Must(v => Encoding.ASCII.GetByteCount(v!) <= opt.MaxHeaderValueBytes)
                .WithMessage(m => $"Header value too long (>{opt.MaxHeaderValueBytes} bytes ASCII).");
        });
    }

    private static bool IsAscii(string input) =>
        !string.IsNullOrEmpty(input) && input.All(c => c <= 127);
}
