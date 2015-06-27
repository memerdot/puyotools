# Introduction #

Proprietary image files are sometimes hard to work through. To Windows they are nothing more than blobs of binary. However, Windows Explorer uses COM to allow other developers to 'extend' it's functionality. Using COM, we can create a 'thumbnail provider' which will allow Windows to create thumbnails for Puyo Tools-compatible imagery.

## Potential Pitfall ##
When Thumbnail Provider is loaded into COM, an instance of .NET 2.0 (.NET 3.5 is only an extension for .NET 2.0's runtime, so yes, .NET 2.0) will be loaded into the process.

This can become a problem if you install software that loads .NET 1.x into Explorer. It's possible that if a .NET 1.x COM object is loaded first, the thumbnail provider will run under .NET 1.x and crash. .NET 2.0 and up, potentially minus .NET 4.x, will not affect thumbnailer's ability to work.

# Implementation #
In Vista+, it is no longer possible to initialize by path. Instead we're given a stream. This stream does contain one bit of file information, the filename, but not the location of this file.

This quickly becomes a problem. Without the full path, it is impossible to determine the location of external palettes.

The solution chosen is simple. We install Thumbnail Provider to also handle palette files. When it comes across a palette file, it dumps it to a special location using only it's filename. When coming across an image with an external palette, we look in this special location for similarly named files.

There are a few drawbacks to this solution.

  * If thumbnails become cached and multiple palettes have the same filename, the palette will stop changing on the last cached thumbnail with the same name.
  * During the first load of a folder, some files may not load. If the cache stores this status, simply reloading the page might not fix the problem.
  * It is difficult to avoid collisions in reading/writing to the same palette file when the thumbnail provider is being executed many times concurrently.

However, it does work in limited testing. It works best when users forcefully disable thumbnail caching, by changing the security permissions on the folder used to store thumbnails.

# Installation #
After compiling the thumbnail provider, enter the 'util' folder. There you will find a readme - this readme will tell you how to install the thumbnail provider and start using it straight from the codebase.