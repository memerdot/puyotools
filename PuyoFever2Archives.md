# Puyo Puyo Fever 2 Archive Formats #

Puyo Puyo Fever 2 uses 3 different archive format to store its data:
  * **MRG** (general purpose archive format)
  * **TEX** (used for storing textures)
  * **SPK** (used for storing sounds)

As their formats are mostly identical, they will all be covered here.

## File Header ##
<pre>
0x0 - 0x3: Magic code (4 bytes)<br>
"MRG0" (if it is a MRG)<br>
"SND0" (if is is a SPK)<br>
"TEX0" (if it is a TEX)<br>
0x4 - 0x7: Number of files in the archive (4 bytes)<br>
0x8 - 0xF: null (16 bytes)<br>
</pre>

## File Table ##
Repeat for however many files are in the archive.

<pre>
0x00 - 0x03: File extension (null terminated) (4 bytes)<br>
0x04 - 0x07: Offset in the archive (4 bytes)<br>
0x08 - 0x0B: File size (4 bytes)<br>
0x0C - 0x0F: null (4 bytes)<br>
0x10 - 0x24/0x30 - File name without extension (null terminated; Shift JIS encoded)<br>
32 bytes if it is a MRG<br>
20 bytes if it is a SPK or TEX<br>
</pre>

## File Data ##
File data is stored using a block size of 16 bytes.