@echo off
setlocal enabledelayedexpansion

for /f "delims=" %%F in ('dir /b /a-d *SonOfRobin* ^| findstr /v /i ".sln"') do (
    set "filename=%%F"
    
    for /f "delims=" %%R in ('powershell -Command "$filename = '%%F'; $index = $filename.IndexOf('SonOfRobin'); if ($index -ge 0) { $newname = $filename.Remove($index, 'SonOfRobin'.Length).Insert($index, 'SonOfRobin_demo') } else { $newname = $filename }; Write-Output $newname"') do (
        set "newname=%%R"
    )

    if not "!newname!" == "" if not "!newname!" == "!filename!" (
        ren "!filename!" "!newname!"
        echo Renamed: "!newname!"
    ) else (
        echo Failed to rename: "!filename!" to "!newname!"
    )
)
