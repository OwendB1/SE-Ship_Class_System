:: This script creates a symlink to the game binaries to account for different installation directories on different systems.

@echo off
set /p path="Please enter the folder location of your SpaceEngineersLauncher.exe: "
cd %~dp0
rmdir GameBinaries > nul 2>&1
mklink /J GameBinaries "%path%"
if errorlevel 1 goto Error
echo Done!
goto End
:Error
echo An error occured creating the symlink.
goto EndFinal
:End
pause

