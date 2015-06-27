# Introduction #

VDD is a container format that is only used in Puyo Puyo Box. The header and files are stored in blocks of 2048 bytes.

## Header ##
0x00 - 0x03
> (4 bytes) Number of files in the VDD archive.

## Filename, File Offsets & Length (starting at 0x04) ##
This is repeated for the number of files in the archive.

0x00 - 0x0F
> (16 bytes) Filename of the file.

0x10 - 0x13
> (4 bytes) Offset of the file in the archive divided by 2048.

0x14 - 0x17
> (4 bytes) Size of the file in the archive.