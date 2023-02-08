C:
cd SonOfRobin.DesktopGL

dotnet publish --runtime linux-x64 -c Release -o Publish -p:PublishSingleFile=true /p:PublishReadyToRun=false --self-contained

attrib +R +X ".\Publish\SonOfRobin.DesktopGL"

cd ..

7z.exe a -ttar SonOfRobin_Linux.tar .\SonOfRobin.DesktopGL\Publish\SonOfRobin.DesktopGL -mx=5
7z.exe a SonOfRobin_Linux.tar.gz SonOfRobin_Linux.tar -mx=5

del SonOfRobin_Linux.tar

rmdir /S /Q .\SonOfRobin.DesktopGL\Publish
