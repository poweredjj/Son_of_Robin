C:
cd SonOfRobin.DesktopGL

rmdir /S /Q Son_of_Robin
rmdir /S /Q Publish

dotnet publish --runtime linux-x64 -c Release -p:PublishSingleFile=true -o Publish /p:PublishReadyToRun=false --self-contained
::-p:PublishSingleFile=true must be on, it won't run on linux otherwise

attrib +R +X ".\Publish\SonOfRobin.DesktopGL"
move /Y Publish Son_of_Robin

cd ..

7z.exe a -ttar SonOfRobin_Linux.tar .\SonOfRobin.DesktopGL\Son_of_Robin\ -mx=5
7z.exe a SonOfRobin_Linux.tar.gz SonOfRobin_Linux.tar -mx=5

del SonOfRobin_Linux.tar
rmdir /S /Q .\SonOfRobin.DesktopGL\Son_of_Robin
