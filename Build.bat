@echo off
mode con: cols=70 lines=50
set root=%cd%

cd "%root%/Huppy/Huppy.App"

echo === Building Arm32 ===  
dotnet publish -r linux-arm -p:PublishSingleFile=true -o %root%/Release/Arm32

echo === Building Windows64 ===
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --output %root%/Release/Windowsx64