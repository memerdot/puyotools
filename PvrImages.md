# Introduction #

PVR images are the native texture format used on the Dreamcast. Almost every game uses this image format.

## Color Formats ##
**Argb1555** dedicates 1 bit for the Alpha component (either completely opaque or completely transparent), and 5 bits for each of the Red, Green, and Blue components.
```
 A | RRRRR | GGGGG | BBBBB
```

**Rgb565** dedicates 5 bits for the Red component, 6 bits for the Green component, and 5 bits for the Blue component.
```
 RRRRR | GGGGGG | BBBBB
```

**Argb4444** dedicates 4 bits for the Alpha, Red, Green, and Blue components.
```
 AAAA | RRRR | GGGG | BBBB
```


# File Format #
The file starts off with 0x20 (32) bytes of header data which describes the remainder of the image.

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
      * 00: Argb1555
      * 01: Rgb565
      * 02: Argb4444

0x19

> Data Format
> > Known Values:
      * 01: Square Twiddled
      * 02: Square Twiddled + Mipmaps
      * 03: VQ Twiddled
      * 04: VQ Twiddled + Mipmaps
      * 05: 4-bit Rectangular Twiddled with external palette
      * 07: 8-bit Rectangular Twiddled with external palette
      * 09: Rectangle
      * 0D: Rectangular Twiddled
      * 12: Square Twiddled + Mipmaps (identical to 02)

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