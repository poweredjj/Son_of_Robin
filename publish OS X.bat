:: Packages a dotnet core MonoGame Desktop GL project as a runnable OSX app in bin/Release/osx-64/
::
:: Author: James Closs | james[at]bitbull.com
::
:: Please also read the section on package apps for OSX here https://docs.monogame.net/articles/packaging_games.html
::
:: Generally you can just replace 'SonOfRobin' with the name of your game
::


C:
cd SonOfRobin.DesktopGL


:: Publish the project - files are output to bin\Release\netcoreapp3.1\$PLATFORM\publish\ 
dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained

:: Remove output directory if it exists

rd /s /q bin\Release\osx-64\

:: Create output directory
mkdir bin\Release\osx-64

:: Create main app package folder
mkdir bin\Release\osx-64\SonOfRobin.app

:: Create necessary subfolders
mkdir bin\Release\osx-64\SonOfRobin.app\Contents
mkdir bin\Release\osx-64\SonOfRobin.app\Contents\Resources
mkdir bin\Release\osx-64\SonOfRobin.app\Contents\MacOS

:: Copy all MonoGame content files to the correct location
copy bin\Release\netcoreapp3.1\osx-x64\publish\Content bin\Release\osx-64\SonOfRobin.app\Contents\Resources

:: Copy all .dlls etx to the correct location

xcopy "bin\Release\netcoreapp3.1\osx-x64\publish" "bin\Release\osx-64\SonOfRobin.app\Contents\MacOS" /s /e

::
:: NOTE If using additional native libs such as required for Steamworks.net these must be copied to Contents\MacOS
:: If not using additional native libs you can ignore this line
::
::copy bin\Release\netcoreapp3.1\osx-x64\publish\runtimes\osx\native\libsteam_api.dylib bin\Release\osx-64\SonOfRobin.app\Contents\MacOS\

:: Remove the content directory from Contents\MacOS - it have been copied with everything else and we don't need it there 

:: Copy the Info.plist file to the correct place (you may have have it somewhere other than resources\osx\)
copy resources\osx\Info.plist bin\Release\osx-64\SonOfRobin.app\Contents\

:: Copy the Icon file to the correct place (you may have have it somewhere other than resources\osx\)
copy resources\osx\Icon.icns bin\Release\osx-64\SonOfRobin.app\Contents\Resources\

:: If debugging Steamworks APIs you may need this file, otherwise you can ignore this line
::copy resources\osx\steam_appid.txt bin\Release\osx-64\SonOfRobin.app\Contents\MacOS\

move bin\Release\osx-64\SonOfRobin.app\Contents\MacOS\Content bin\Release\osx-64\SonOfRobin.app\Contents\Resources

cd ..

attrib +R +X ".\SonOfRobin.DesktopGL\bin\Release\osx-64\SonOfRobin.app\Contents\MacOS\SonOfRobin.DesktopGL"

7z.exe a -ttar SonOfRobin_OSX.tar ".\SonOfRobin.DesktopGL\bin\Release\osx-64\SonOfRobin.app" -mx=5
7z.exe a SonOfRobin_OSX.tar.gz SonOfRobin_OSX.tar -mx=5

del SonOfRobin_OSX.tar

rmdir /S /Q .\SonOfRobin.DesktopGL\bin\Release\netcoreapp3.1
rmdir /S /Q .\SonOfRobin.DesktopGL\bin\Release\osx-64
