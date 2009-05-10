using System;

namespace puyo_tools
{
    class CompressionFormatNotSupported : Exception
    {
        public CompressionFormatNotSupported()
        {
        }
    }

    class ArchiveFormatNotSupported : Exception
    {
        public ArchiveFormatNotSupported()
        {
        }
    }

    class GraphicFormatNotSupported : Exception
    {
        public GraphicFormatNotSupported()
        {
        }
    }

    class IncorrectGraphicFormat : Exception
    {
        public IncorrectGraphicFormat()
        {
        }
    }
}