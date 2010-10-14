@echo off
echo Registering COM objects... this may take a moment.
regasm32 /codebase ..\bin\debug\pt_thumb.dll
regasm64 /codebase ..\bin\debug\pt_thumb.dll
echo Okay, all of the commands have been executed. If there are any errors above, you may need to correct them. Test your thumbnailer by browsing into a folder with files supported by the Puyo Tools Image Viewer using Windows Explorer.
pause