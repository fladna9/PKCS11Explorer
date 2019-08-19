@echo off
echo Building Windows Portable x86
dotnet publish -c Release --self-contained true -f netcoreapp2.1 -r win-x86
echo Building Windows Portable x64
dotnet publish -c Release --self-contained true -f netcoreapp2.1 -r win-x64
echo Building macOS Portable x64
dotnet publish -c Release --self-contained true -f netcoreapp2.1 -r osx-x64
echo Building Linux Portable ARM
dotnet publish -c Release --self-contained true -f netcoreapp2.1 -r linux-arm
echo Building Linux Portable x64
dotnet publish -c Release --self-contained true -f netcoreapp2.1 -r linux-x64