@echo off
rem Automatic build from the command line (same methods as the Build menu in the editor):
rem   build.bat           - Build All: PC (Windows x64) + Android (APK)
rem   build.bat Windows   - PC only
rem   build.bat Android   - Android only
rem   build.bat WebGL     - WebGL with the custom template Assets/WebGLTemplates/Modern
rem The editor is taken from the Unity Hub folder for the version in ProjectSettings\ProjectVersion.txt;
rem set UNITY_EXE to use another one, e.g.  set UNITY_EXE=E:\Unity\Editors\6000.3.8f1\Editor\Unity.exe
setlocal
set "PROJECT=%~dp0"
set "TARGET=%~1"
if "%TARGET%"=="" set "TARGET=All"
if not defined UNITY_EXE (
  for /f "tokens=2" %%v in ('findstr /b "m_EditorVersion:" "%PROJECT%ProjectSettings\ProjectVersion.txt"') do set "VERSION=%%v"
)
if not defined UNITY_EXE set "UNITY_EXE=%ProgramFiles%\Unity\Hub\Editor\%VERSION%\Editor\Unity.exe"
if not exist "%UNITY_EXE%" (
  echo Unity not found: "%UNITY_EXE%"
  echo Set the path first:  set UNITY_EXE=C:\...\Editor\Unity.exe
  exit /b 2
)
if not exist "%PROJECT%Builds" mkdir "%PROJECT%Builds"
echo Building %TARGET% with "%UNITY_EXE%" ...
"%UNITY_EXE%" -batchmode -quit -projectPath "%PROJECT%." -executeMethod BuildPractice.Editor.BuildScript.Build%TARGET% -logFile "%PROJECT%Builds\build-%TARGET%.log"
set "CODE=%ERRORLEVEL%"
if "%CODE%"=="0" (
  echo Done: see the Builds folder.
) else (
  echo Build failed ^(%CODE%^): see Builds\build-%TARGET%.log
)
exit /b %CODE%
