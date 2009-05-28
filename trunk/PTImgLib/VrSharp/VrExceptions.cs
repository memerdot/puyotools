using System;
using System.Collections.Generic;
using System.Text;

namespace VrSharp
{
    public class VrNoSuitableCodecException : _ErrorException
    {
        public VrNoSuitableCodecException(string errorMessage) : base(errorMessage) {}

        public VrNoSuitableCodecException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
    public class VrCodecProcessingException : _ErrorException
    {
        public VrCodecProcessingException(string errorMessage) : base(errorMessage) { }

        public VrCodecProcessingException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
    public class VrCodecHeaderException : _ErrorException
    {
        public VrCodecHeaderException(string errorMessage) : base(errorMessage) { }

        public VrCodecHeaderException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
    public class VrCodecLoadingException : _ErrorException
    {
        public VrCodecLoadingException(string errorMessage) : base(errorMessage) { }

        public VrCodecLoadingException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
    public class NotVrException : _ErrorException
    {
        public NotVrException(string errorMessage) : base(errorMessage) { }

        public NotVrException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
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
