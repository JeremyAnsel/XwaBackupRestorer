@echo off
setlocal

cd "%~dp0"

For %%a in (
"XwaBackupRestorer\bin\Release\net48\*.dll"
"XwaBackupRestorer\bin\Release\net48\*.exe"
"XwaBackupRestorer\bin\Release\net48\*.config"
) do (
xcopy /s /d "%%~a" dist\
)
