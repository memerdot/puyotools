# Introduction #

GMP is an image format that has been used in various DiscStation games. Aside from their header, they are identical to BMP images.

## Header ##
0x00 - 0x07: GMP-200
> (8 bytes) Magic Code

0x08 - 0x0B
> (4 bytes) Height of the image in pixels.

0x0C - 0x0F
> (4 bytes) Width of the image in pixels.

0x10 - 0x13
> (4 bytes) 0000 0000 (NULL bytes)

0x14 - 0x17
> (4 bytes) Size of the header (This value is always 32).

0x18 - 0x1B
> (4 bytes) Start of image data.

0x1C - 0x1D
> (2 bytes) Number of colors in the palette.

0x1E - 0x1F
> (2 bytes) Bit-Depth of the image.

## Palette Information ##
**This section assumes the GMP is an 8-bit image.**

This data comes directly after the header.
Colors in the palette are stored in RGBX8888:

```
 | BBBBBBBB | GGGGGGGG | RRRRRRRR | 00000000
```

It is repeated for the number of colors in the image.

## Image Data ##
**This section assumes the GMP is an 8-bit image.**

Image data in GMP files are stored identically to that in BMP files, in that the first byte refers to the bottom-most left pixel, and the last byte refers to the top-most right pixel. Each byte refers to a color in the palette.