# GVR Texture Format #

GVR is the PVR like texture format used on both Gamecube and Wii. It's essentially a standard Gamecube/Wii TPL but with a PVR header instead of a TPL header.

## Pixel Formats ##
Used only when using an palette based format (when data format is 0x8 or 0x9).

  * **0x0** - 8-bit intensity with alpha
  * **0x1** - RGB565
  * **0x2** - RGB5A3 (RGB555 if the most significant bit is 1, RGB4443 otherwise)

## Data Formats ##
  * **0x0** - 4-bit intensity
  * **0x1** - 8-bit intensity
  * **0x2** - 4-bit intensity with alpha
  * **0x3** - 8-bit intensity with alpha
  * **0x4** - RGB565
  * **0x5** - RGB5A3 (RGB555 if the most significant bit is 1, RGB4443 otherwise)
  * **0x6** - RGBA8888
  * **0x8** - 4-bit indexed palette (see pixel formats)
  * **0x9** - 8-bit index palette (see pixel formats)
  * **0xE** - CMP (DTX1 encoded)

## File Format ##
### GBIX Header (optional) ###
The GBIX/GCIX header is always the first thing in the GVR texture. This header is optional.

<pre>
0x0 - 0x3: "GBIX" (for Gamecube) (4 bytes)<br>
0x0 - 0x3: "GCIX" (for Wii) (4 bytes)<br>
0x4 - 0x7: 8 (4 bytes)<br>
0x8 - 0xB: Global index (big endian) (4 bytes)<br>
0xC - 0xF: null (4 bytes)<br>
</pre>

### GVRT Header ###
The GVRT header is stored directly after the GBIX/GCIX header (if it exists).

<pre>
0x0 - 0x3: "GVRT" (4 bytes)<br>
0x4 - 0x7: data offset + data size - 24 (4 bytes)<br>
0x8 - 0x9: null (2 bytes)<br>
0xA: (1 byte)<br>
Upper 4 bits: Pixel format (0 if not using a palette based format) (4 bits)<br>
Lower 4 bits: Flags (see below) (4 bits)<br>
0x1 - Texture contains mipmaps<br>
0x2 - Texture contains external CLUT<br>
0x8 - Texture contains embedded CLUT<br>
0xB: Data format (1 byte)<br>
0xC - 0xD: Texture width (big endian) (2 bytes)<br>
0xE - 0xF: Texture height (big endian) (2 bytes)<br>
</pre>

### CLUT ###
If the file contains an internal/embedded CLUT, it would be stored after the GVRT header and before the texture data

### Texture Data ###
Texture data is stored after the CLUT/GVRT header. How it's stored depends on the data format.