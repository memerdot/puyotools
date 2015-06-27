# Introduction #

ACX is a container for ADX files. This is a common format that has been used in many non-Puyo Puyo games. The header and files are stored in blocks of 4 bytes.


## Header ##
0x00 - 0x03: 0000 0000
> (4 bytes) Magic Code

0x04 - 0x07
> (4 bytes) Number of files in the ACX archive (Big Endian).

## File Offsets & Length (starting at 0x08) ##
This is repeated for the number of files in the archive.

0x00 - 0x03
> (4 bytes) Offset of the file in the archive (Big Endian).

0x04 - 0x07
> (4 bytes) Size of the file in the archive (Big Endian).