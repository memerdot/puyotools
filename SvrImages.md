# Introduction #

SVR files are the image files used by Sonic Team (Japan) on the Playstation 2. They are similar to PVR used on the Dreamcast but suited for Playstation 2.

## Color Formats ##
Note: SVR images are stored with their color channels swapped (BGRA instead of ARGB; BGR instead of RGB).

**Bgr5a3** is Bgr555 when the first bit is one, and Abgr4443 when the first bit is 0.
```
 1 | BBBBB | GGGGG | RRRRR |
 0 | BBBB  | GGGG  | RRRR  | AAA
```

**BGRA8888** indicates that there are 8 bits for each for the Blue, Green, Red, and Alpha components. Despite taking up 8 bits, the alpha channel only contains 128 possible values, with 0x80 being fully opaque and 0x00 being fully transparent. The blue, green, and red color channels contain 256 possible values however.
```
 BBBBBBBB | GGGGGGGG | RRRRRRRR | AAAAAAAA
```


# File Format #
The file starts off with 0x20 (64) bytes of header data which describes the remainder of the image.

## Header ##
The header consists of two main parts, each typically consisting of 0x10 bytes. The first header is not required, and as a result may not exist in the file.

### 1st Header ###
0x00 - 0x03   'GBIX'
> Magic Code, 4 bytes

0x04 - 0x07   Global Index
> 4 bytes.

0x08 - 0x0F
> Reserved - 8 bytes. Always Zero.


### 2nd Header ###
0x10 - 0x13   'PVRT'
> Format Magic Code, 4 bytes

0x14 - 0x17
> Size, 4 bytes. Looks like the post-header filesize but I just ignore it for decoding.

0x18
> Palette Format
> > Known Values:
      * 08: BGR5A3
      * 09: BGRA8888

0x19

> Data Format
> > Known Values:
      * 60: Non-palette based image
      * 62: 4-bit palettized format, palette stored in a seperate SVP file.
      * 64: 8-bit palettized format, palette stored in a seperate SVP file.
      * 66: 4-bit palettized format
      * 68: 4-bit palettized format
      * 69: 4-bit palettized format
      * 6A: 8-bit palettized format
      * 6C: 8-bit palettized format
      * 6D: 8-bit palettized format

0x1A - 0x1B

> 2 bytes. Always zero.

0x1C - 0x1D
> Width, 2 bytes

0x1E - 0x1F
> Height, 2 bytes


## Format Header ##
This is where the palette is stored. Refer to the palette format for more information.

## Image Data ##
Refer to the data format for more information on how the data is stored as.