using System;
using System.Runtime.Serialization;

namespace OFXSharp
{
    [Serializable]
    public class OFXParseException : OFXException
    {
        public OFXParseException()
        {
        }

        public OFXParseException(string message) : base(message)
        {
        }

        public OFXParseException(string message, Exception inner) : base(message, inner)
        {
        }

        protected OFXParseException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}