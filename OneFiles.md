# Introduction #

ONE is a container for any kind of files, and is used heavily in Sonic Unleashed. ONZ compressed ONE archives use the file extension .onz. The header and files are stored in blocks of 32 bytes.<br />
**ONE files in Shadow the Hedgehog use a different format and are not supported. However, support will likely be added in the future.**

## Header ##
0x00 - 0x03: 'one.' (6F 6E 65 2E)
> (4 bytes) Magic Code

0x04 - 0x07
> (4 bytes) Number of files in the archive.

## Filename, File Offsets & Length (starting at 0x08) ##
This is repeated for the number of files in the archive.

0x00 - 0x37
> (56 bytes) Filename of the file.

0x38 - 0x3B
> (4 bytes) Offset of the file in the archive.

0x3C - 0x3F
> (4 bytes) Size of the file in the archive.