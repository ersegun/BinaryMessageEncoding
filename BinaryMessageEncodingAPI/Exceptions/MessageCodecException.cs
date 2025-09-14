namespace BinaryMessageEncodingAPI.Exceptions
{
    [Serializable]
    public class MessageCodecException : Exception
    {
        public MessageCodecException() { }

        public MessageCodecException(string message) : base(message) { }

        public MessageCodecException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
