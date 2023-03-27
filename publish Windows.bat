C:
cd SonOfRobin.WindowsDX
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

copy /Y "..\_legacy_fonts\*" "bin\Release\net6.0-windows\win-x64\publish\Content\fonts\"

cd ..

move .\SonOfRobin.WindowsDX\bin\Release\net6.0-windows\win-x64\publish ".\SonOfRobin.WindowsDX\bin\Release\net6.0-windows\win-x64\Son_of_Robin"

7z.exe a SonOfRobin_Windows.zip ".\SonOfRobin.WindowsDX\bin\Release\net6.0-windows\win-x64\Son_of_Robin" -tzip -mx=9

rmdir /S /Q .\SonOfRobin.WindowsDX\bin\Release\net6.0-windows\win-x64