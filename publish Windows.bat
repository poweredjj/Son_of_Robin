C:
cd SonOfRobin.WindowsDX
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

cd ..

move .\SonOfRobin.WindowsDX\bin\Release\net8.0-windows7.0\win-x64\publish ".\SonOfRobin.WindowsDX\bin\Release\net8.0-windows7.0\win-x64\Son_of_Robin"

7z.exe a SonOfRobin_Windows.zip ".\SonOfRobin.WindowsDX\bin\Release\net8.0-windows7.0\win-x64\Son_of_Robin" -tzip -mx=9

cd "_batches"
..\7z a -tzip ..\SonOfRobin_Windows.zip "Son of Robin.bat"
cd ..

rmdir /S /Q .\SonOfRobin.WindowsDX\bin\Release\net8.0-windows7.0\win-x64