cd SonOfRobin.WindowsDX
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

cd ..

7z.exe a SonOfRobin_Windows.7z .\SonOfRobin.WindowsDX\bin\Release\netcoreapp3.1\win-x64\publish -t7z -m0=lzma2:d1024m -mx=9 -aoa -mfb=64 -md=32m -ms=on

rmdir /S /Q .\SonOfRobin.WindowsDX\bin\Release\netcoreapp3.1\win-x64
