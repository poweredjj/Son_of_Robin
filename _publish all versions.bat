powershell -Command "(gc SonOfRobin.Core\Game.cs) -replace '   if \(ThisIsWorkMachine\) game\.Window\.Position', '   // if (ThisIsWorkMachine) game.Window.Position' | Out-File -encoding ASCII SonOfRobin.Core\Game.cs"

powershell -Command "(gc SonOfRobin.Core\Game.cs) -replace 'trialVersion = false;', 'trialVersion = true;' | Out-File -encoding ASCII SonOfRobin.Core\Game.cs"

call "publish Windows.bat"
call "publish Linux.bat"
call "publish Linux ARM.bat"
call "publish OS X x86.bat"
call "publish Android.bat"

call "_rename_packages_as_demo.bat"

powershell -Command "(gc SonOfRobin.Core\Game.cs) -replace 'trialVersion = true;', 'trialVersion = false;' | Out-File -encoding ASCII SonOfRobin.Core\Game.cs"

call "publish Windows.bat"
call "publish Linux.bat"
call "publish Linux ARM.bat"
call "publish OS X x86.bat"
call "publish Android.bat"