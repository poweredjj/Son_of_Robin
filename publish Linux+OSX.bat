cd SonOfRobin.DesktopGL

dotnet publish -c Release -r linux-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained
dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

cd ..

move .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64\publish ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64\Son of Robin"
move .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64\publish ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64\Son of Robin"

7z.exe a SonOfRobin_Linux.7z ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64\Son of Robin" -tzip -mx=9
7z.exe a SonOfRobin_OSX.7z ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64\Son of Robin" -tzip -mx=9

rmdir /S /Q .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\linux-x64
rmdir /S /Q .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64
