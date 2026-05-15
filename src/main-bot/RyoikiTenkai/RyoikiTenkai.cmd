@echo off
rmdir /s /q bin obj >nul 2>&1
dotnet build
dotnet run --no-build