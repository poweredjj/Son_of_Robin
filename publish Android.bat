C:
cd SonOfRobin.Android

dotnet publish -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=AhoyGames.keystore -p:AndroidSigningKeyAlias=AhoyGames -p:AndroidSigningKeyPass=Korzen321 -p:AndroidSigningStorePass=Korzen321

move bin\Release\net8.0-android\publish\SonOfRobin.SonOfRobin-Signed.apk ..\

cd ..