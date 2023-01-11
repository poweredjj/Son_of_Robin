C:
cd SonOfRobin.DesktopGL

dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

cd ..

move .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64\publish ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64\Son_of_Robin"

attrib +R +X ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64\Son_of_Robin\SonOfRobin.DesktopGL"

7z.exe a SonOfRobin_OSX.zip ".\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64\Son_of_Robin" -tzip -mx=9

rmdir /S /Q .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64
