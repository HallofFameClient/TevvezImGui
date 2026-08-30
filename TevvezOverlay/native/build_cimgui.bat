@echo off
rem ===================================================================
rem  cimgui als statische Bibliothek bauen (fuer NativeAOT)
rem
rem  Aufruf:  build_cimgui.bat <pfad-zum-cimgui-checkout>
rem
rem  Vorher einmalig:
rem    git clone --recurse-submodules https://github.com/cimgui/cimgui.git
rem    cd cimgui
rem    git checkout 970c614          rem = "pull imgui docking 1.91.6"
rem    git submodule update --init --recursive
rem
rem  Wichtig sind zwei Schalter:
rem    CIMGUI_NO_EXPORT               kein __declspec(dllexport) - wir bauen
rem                                   ja keine DLL
rem    IMGUI_DISABLE_OBSOLETE_FUNCTIONS
rem                                   genau so baut ImGui.NET seine cimgui.
rem                                   Ohne den Schalter ist ImGuiIO 24 Bytes
rem                                   groesser und die Strukturen passen nicht
rem                                   mehr zu den P/Invoke-Angaben.
rem ===================================================================

if "%~1"=="" (
  echo Aufruf: build_cimgui.bat ^<pfad-zum-cimgui-checkout^>
  exit /b 1
)
set "SRC=%~f1"
if not exist "%SRC%\cimgui.cpp" (
  echo cimgui.cpp nicht gefunden unter "%SRC%"
  exit /b 1
)
if not exist "%SRC%\imgui\imgui.cpp" (
  echo imgui-Submodul fehlt - "git submodule update --init --recursive" vergessen?
  exit /b 1
)

set "PATH=C:\Program Files (x86)\Microsoft Visual Studio\Installer;%PATH%"
call "%ProgramFiles%\Microsoft Visual Studio\18\Insiders\VC\Auxiliary\Build\vcvars64.bat" >nul
if errorlevel 1 (
  echo vcvars64 nicht gefunden - Pfad im Skript anpassen
  exit /b 1
)

set "OUT=%~dp0"
pushd "%SRC%"
if not exist build_static mkdir build_static
cd build_static

cl /c /nologo /EHsc /MT /O2 /W0 /I.. /I..\imgui ^
   /DCIMGUI_NO_EXPORT /DIMGUI_DISABLE_OBSOLETE_FUNCTIONS ^
   ..\cimgui.cpp ..\imgui\imgui.cpp ..\imgui\imgui_draw.cpp ^
   ..\imgui\imgui_demo.cpp ..\imgui\imgui_tables.cpp ..\imgui\imgui_widgets.cpp
if errorlevel 1 (popd & echo COMPILE FEHLER & exit /b 1)

lib /nologo /OUT:"%OUT%cimgui.lib" *.obj
if errorlevel 1 (popd & echo LIB FEHLER & exit /b 1)

popd
echo.
echo Fertig: %OUT%cimgui.lib
