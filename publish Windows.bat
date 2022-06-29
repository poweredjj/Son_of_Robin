C:
cd SonOfRobin.WindowsDX
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

cd ..

move .\SonOfRobin.WindowsDX\bin\Release\netcoreapp3.1\win-x64\publish ".\SonOfRobin.WindowsDX\bin\Release\netcoreapp3.1\win-x64\Son of Robin"

7z.exe a SonOfRobin_Windows.zip ".\SonOfRobin.WindowsDX\bin\Release\netcoreapp3.1\win-x64\Son of Robin" -tzip -mx=9

rmdir /S /Q .\SonOfRobin.WindowsDX\bin\Release\netcoreapp3.1\win-x64