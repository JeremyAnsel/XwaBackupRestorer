@echo off
setlocal

cd "%~dp0"

For %%a in (
"XwaBackupRestorer\bin\Release\net40\*.dll"
"XwaBackupRestorer\bin\Release\net40\*.exe"
"XwaBackupRestorer\bin\Release\net40\*.config"
) do (
xcopy /s /d "%%~a" dist\
)
