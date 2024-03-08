powershell -Command "Get-ChildItem *SonOfRobin* -File | ForEach-Object { $newName = $_.Name -replace 'SonOfRobin', 'SonOfRobin_demo'; Rename-Item $_.FullName $newName }"
