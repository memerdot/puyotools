================================================================================================
PuyoPuyo! Tools ImgLib (PTImgLib)
------------------------------------------------------------------------------------------------
Released under the New BSD license - See license.txt for details
================================================================================================


- Preface --------------------------------------------------------------------------------------
I wrote this software to read and write Gvr files from C# at a decent speed. I used every trick in the book for optimization, except the ones that I didn't use - Aka all of the ones I didn't know about. So it probably sucks. But hey - It will get the work done and hopefully this one won't be so poorly written so Yoshi can read it better than that P.O.S. I wrote in C. I'm sure If I wouldn't have written it so terribly, he could have read it any ways - But this solution allows him to use it as a library, and without Pinvoke - So i think it was a good decision.

If anyone ever reads this aside from me, then enjoy...


- Table of Contents ----------------------------------------------------------------------------
I. Compilation
 A. Software Needed
 B. Dependencies

II. Usage
 A. GvrConv

III. The GVR Format
 A. The Header
 B. STUB!


- Compilation ----------------------------------------------------------------------------------
I *really* don't want to piss anyone off with my multiplatform obsession, but I think it's important that nobody gets left in the dark with final releases. So please bare with me on Mono compatibility, Even though I do have an XP box up and running now I feel that OS X and Linux users should be able to use this software, so if it means a few changes before a release I'm glad to do it.


A. Software Needed

In order to compile PTImgLib, You must have installed, Microsoft Visual C# 2005 Express Edition or MonoDevelop. Project files for both are provided though at times compatibility with Mono may be dropped off temporarily.

In Microsoft Visual C# Express 2005 and MonoDevelop, you can simply open the solution and compile. If you use the command line "gcms" or Microsofts Official C# compiler, you will need to manually enter in the file names. Typically just compile all of the C# files in a single folder.


B. Dependencies

At this time all dependencies are provided - The only external dependency for is for GvrSharp's quantizer (BetterImageProcessor's) and it is included.


- Usage ----------------------------------------------------------------------------------------
With PTImgLib comes a brand new GvrConv. It works similarly to the old one but with more functionality, like outputting Gvr files. With this new functionality comes added syntax.


A. GvrConv

GvrConv <Input> [Output]

Input: Required, Input Filename
Output: Optional, Output Filename.

To specify how to make a new Gvr file, you must specify an existing Gvr file or a Pixel Format to use (If you choose an existing Gvr file, the palette will stay intact, if not, GvrSharp shall quantize the image using BetterImageProcessor for you.)


- The GVR Format -------------------------------------------------------------------------------
A. Header

1st Header

0x00 - 0x03 'GCIX'
    Magic Code, 4 bytes 

0x04 - 0x07 Unknown
    4 bytes. Possibly useless, Possibly offset to Image Header 2. (Seemingly always 8) 

0x08 - 0x0F
    Reserved - 8 bytes. Always Zero, Probably usable for metadata. 


2nd Header

0x10 - 0x13 'GVRT'
    Format Magic Code, 4 bytes 

0x14 - 0x17
    Size, 4 bytes. Looks like the post-header filesize but I just ignore it for decoding. 

0x18 - 0x1B
    Pixel Format, 4 bytes. The first two bytes aren't used as far as I know.

        Known Values:
            * Rgb5a3 Pixels, data in 8x4 chunks = 0x0005
            * 256 Color Palette with Rgb565 colors, indexed data in 8x4 chunks = 0x1809
            * 16 Color Palette per chunk with Rgb5a3 colors, data in 8x4 chunks = 0x2809
            * 16 Color Palette per chunk with Rgb5a3 colors, data in 8x8 chunks = 0x2808 

0x1C - 0x1D
    Width, 2 bytes 

0x1E - 0x1F
    Height, 2 bytes 


B. STUB!
Stub for more information. Sorry.