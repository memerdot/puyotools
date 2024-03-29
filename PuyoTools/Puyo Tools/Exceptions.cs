﻿using System;

namespace PuyoTools
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

    class GraphicFormatNeedsPalette : Exception
    {
        public GraphicFormatNeedsPalette()
        {
        }
    }
}