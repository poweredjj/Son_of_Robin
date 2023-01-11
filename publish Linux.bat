C:
cd SonOfRobin.DesktopGL

dotnet publish -c Release -r linux-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

cd ..

move .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64\publish ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64\Son_of_Robin"

attrib +R +X ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64\Son_of_Robin\SonOfRobin.DesktopGL"

7z.exe a SonOfRobin_Linux.zip ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64\Son_of_Robin" -tzip -mx=9

rmdir /S /Q .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64
