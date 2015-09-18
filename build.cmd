@echo off

call paket.cmd restore
if errorlevel 1 (
  exit /b %errorlevel%
)

packages\build\FAKE\tools\FAKE.exe "build\build.fsx" %*
