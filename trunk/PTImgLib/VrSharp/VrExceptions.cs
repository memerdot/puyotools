using System;
using System.Collections.Generic;
using System.Text;

namespace GvrSharp
{
    public class GvrNoSuitableCodecException : _ErrorException
    {
        public GvrNoSuitableCodecException(string errorMessage) : base(errorMessage) {}

        public GvrNoSuitableCodecException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
    public class GvrCodecProcessingException : _ErrorException
    {
        public GvrCodecProcessingException(string errorMessage) : base(errorMessage) { }

        public GvrCodecProcessingException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
    public class GvrCodecHeaderException : _ErrorException
    {
        public GvrCodecHeaderException(string errorMessage) : base(errorMessage) { }

        public GvrCodecHeaderException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
    public class GvrCodecLoadingException : _ErrorException
    {
        public GvrCodecLoadingException(string errorMessage) : base(errorMessage) { }

        public GvrCodecLoadingException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
    public class NotGvrException : _ErrorException
    {
        public NotGvrException(string errorMessage) : base(errorMessage) { }

        public NotGvrException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }

    [Serializable]
    public class _ErrorException : Exception
    {
        public string ErrorMessage
        {
            get
            {
                return base.Message.ToString();
            }
        }

        public _ErrorException(string errorMessage)
            : base(errorMessage) { }

        public _ErrorException(string errorMessage, Exception innerEx)
            : base(errorMessage, innerEx) { }
    }
}
