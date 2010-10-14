This is a set of utilities that can be used to install the thumbnail generator inside of the codebase (for testing, obviously.)

However, Windows is really fucking stupid. Like, unbelievably. You need to run install.bat as administrator, but if you try to do so via the context menu, the CWD will be wrong. I'm not sure WHY. I wasn't able to make the batch file execute itself as admin if needed, and further research shows that might not be easy. Tried creating a shortcut, but windows wouldn't let it recieve the acquire admin flag. (I'm assuming shortcuts to batch files are just pifs.)

So, here's how you have to do things in order to install. Luckily, you only need to do it one time, and as of Vista, you can even update the COM object in realtime without any re-registering or restarting of explorer so as long as the object isn't in use (e.g. actually running.)

1. Find 'Command Prompt' in your start menu (it's under (All) Programs -> Accessories.)
2. Contextually click (right click usually) and select "Run as Administrator."
3. type "cd " (note the space) and drag the "util" folder onto the terminal window (e.g. from the "Thumbnail Provider" folder) and hit enter to execute that command.
4. If your util folder is on a different harddrive than Windows, type the letter of the drive the util folder is on and a colon. e.g. D:
5. Type install.bat and hit enter.
6. Wait for the COM objects to register. (PuyoTools.ThumbnailProvider will be registered twice, 32-bit and 64-bit, and each time it will register GimSharp, VrSharp and ImgSharp with it. You won't see them, don't worry.)

At this point, everything SHOULD be good. Test by entering a folder with image files supported by Puyo Tools Image Viewer using Windows Explorer. Enter thumbnail view mode. If all is good and well, you should see thumbnails beginning to populate the folder.