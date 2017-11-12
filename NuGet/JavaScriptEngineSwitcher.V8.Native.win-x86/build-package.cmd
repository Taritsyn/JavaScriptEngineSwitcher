set project_name=JavaScriptEngineSwitcher.V8.Native.win-x86
set lib_dir=..\..\lib\ClearScript
set licenses_dir=..\..\Licenses
set nuget_package_manager=..\..\.nuget\nuget.exe

call "..\setup.cmd"

rmdir runtimes /Q/S

del clearscript-license.txt /Q/S
del v8-license.txt /Q/S

xcopy "%lib_dir%\runtimes\win-x86\native\ClearScriptV8-32.dll" runtimes\win7-x86\native\
xcopy "%lib_dir%\runtimes\win-x86\native\v8-base-ia32.dll" runtimes\win7-x86\native\
xcopy "%lib_dir%\runtimes\win-x86\native\v8-ia32.dll" runtimes\win7-x86\native\

copy "%licenses_dir%\clearscript-license.txt" clearscript-license.txt /Y
copy "%licenses_dir%\v8-license.txt" v8-license.txt /Y

%nuget_package_manager% pack "..\%project_name%\%project_name%.nuspec"