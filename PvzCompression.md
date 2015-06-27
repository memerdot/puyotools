# Introduction #

PVZ Compression is a [RLE](http://en.wikipedia.org/wiki/Run-length_encoding) based compression format used on the PVR images in Puyo Puyo Fever. Due to the nature of how the compression format works, it can only be used on PVR images with specific data formats.

# Format #

PVZ Compression is very basic. It simply is "write this pixel x amount of times in the decompressed image." Let's take the following example of a pixel with the PVR palette format being ARGB4444:

This would write the pixel 1 time.
```
 FFFF 00
```

This would write the pixel 256 times in the decompressed data.
```
 FFFF FF
```