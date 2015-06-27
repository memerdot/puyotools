# Introduction #

AFS is a container for any kind of files, although it usually contains only ADX files. This is a common format that has been used in many non-Puyo Puyo games. The header, files, and metadata are usually stored in blocks of 2048 bytes, although in Puyo Puyo! 15th Anniversary it has a block size of 2.

## Header ##
0x00 - 0x03: 'AFS' (41 46 53 00)
> (4 bytes) Magic Code

0x04 - 0x07
> (4 bytes) Number of files in the archive.

## File Offsets & Length (starting at 0x08) ##
This is repeated for the number of files in the archive.

0x00 - 0x03
> (4 bytes) Offset of the file in the archive.

0x04 - 0x07
> (4 bytes) Size of the file in the archive.

### Metadata Location ###
v1 AFS Archives: This comes right before the data of the first file.<br />
v2 AFS Archives: This comes directly after the File Offsets & Length Data.

0x00 - 0x03
> (4 bytes) Offset where the metadata is located.

0x04 - 0x07
> (4 bytes)  Size of the metadata.

## Metadata (Stored at the end of the archive) ##
This is repeated for the number of files in the archive.

0x00 - 0x1F
> (32 bytes) Filename for the file inside the archive.

0x20 - 0x21
> (2 bytes) Year of Creation Date for the file inside the archive.

0x22 - 0x23
> (2 bytes) Month of Creation Date for the file inside the archive.

0x24 - 0x25
> (2 bytes) Day of Creation Date for the file inside the archive.

0x26 - 0x27
> (2 bytes) Hour of Creation Date for the file inside the archive.

0x28 - 0x29
> (2 bytes) Minute of Creation Date for the file inside the archive.

0x2A - 0x2B
> (2 bytes) Second of Creation Date for the file inside the archive.

0x2C - 0x2F
> (4 bytes) This uses data that is in the header. Assuming X is the current file, it uses the value that is at:
> > `0x08 + (X * 0x08)` (for v1 AFS Archive)<br />
> > `0x04 + (X * 0x04)` (for v2 AFS Archive)