C:
cd SonOfRobin.DesktopGL

rmdir /S /Q Son_of_Robin
rmdir /S /Q Publish

dotnet publish --runtime linux-arm -c Release -p:PublishSingleFile=true -o Publish

::-p:PublishSingleFile=true must be on, it won't run on linux otherwise

attrib +R +X ".\Publish\SonOfRobin.DesktopGL"
move /Y Publish Son_of_Robin

cd ..

7z.exe a -ttar SonOfRobin_Linux_ARM.tar .\SonOfRobin.DesktopGL\Son_of_Robin\ -mx=5
7z.exe a SonOfRobin_Linux_ARM.tar.gz SonOfRobin_Linux_ARM.tar -mx=5

del SonOfRobin_Linux_ARM.tar
rmdir /S /Q .\SonOfRobin.DesktopGL\Son_of_Robin
