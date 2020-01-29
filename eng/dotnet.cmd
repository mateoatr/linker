@echo off
setlocal

powershell -ExecutionPolicy ByPass -NoProfile -File "%~dp0\dotnet.ps1"

endlocal
