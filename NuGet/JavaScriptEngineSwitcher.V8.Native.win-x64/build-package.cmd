set project_name=JavaScriptEngineSwitcher.V8.Native.win-x64
set lib_dir=..\..\lib\ClearScript
set licenses_dir=..\..\Licenses
set nuget_package_manager=..\..\.nuget\nuget.exe

call "..\setup.cmd"

rmdir runtimes /Q/S

del clearscript-license.txt /Q/S
del v8-license.txt /Q/S

xcopy "%lib_dir%\runtimes\win-x64\native\ClearScriptV8-64.dll" runtimes\win-x64\native\
xcopy "%lib_dir%\runtimes\win-x64\native\v8-x64.dll" runtimes\win-x64\native\

copy "%licenses_dir%\clearscript-license.txt" clearscript-license.txt /Y
copy "%licenses_dir%\v8-license.txt" v8-license.txt /Y

%nuget_package_manager% pack "..\%project_name%\%project_name%.nuspec"