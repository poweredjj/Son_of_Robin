cd SonOfRobin.DesktopGL
dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

cd ..

7z.exe a SonOfRobin_OSX.7z .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64\publish -t7z -m0=lzma2:d1024m -mx=9 -aoa -mfb=64 -md=32m -ms=on

rmdir /S /Q .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1\osx-x64

pause