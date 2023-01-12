C:
cd SonOfRobin.DesktopGL

dotnet publish --runtime linux-x64 -c Release -o Publish -p:PublishSingleFile=true /p:PublishReadyToRun=false --self-contained

attrib +R +X ".\Publish\SonOfRobin.DesktopGL"

cd ..

7z.exe a -tzip -r SonOfRobin_Linux_single_file.zip .\SonOfRobin.DesktopGL\Publish\SonOfRobin.DesktopGL -mx=5

rmdir /S /Q .\SonOfRobin.DesktopGL\Publish
