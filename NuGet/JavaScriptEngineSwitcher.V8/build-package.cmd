set project_name=JavaScriptEngineSwitcher.V8
set project_source_dir=..\..\src\%project_name%
set project_bin_dir=%project_source_dir%\bin\Release
set binaries_dir=..\..\Binaries\ClearScript
set licenses_dir=..\..\Licenses
set nuget_package_manager=..\..\.nuget\nuget.exe

call "..\setup.cmd"

rmdir lib /Q/S
rmdir runtimes /Q/S

del clearscript-license.txt /Q/S
del v8-license.txt /Q/S

%net40_msbuild% "%project_source_dir%\%project_name%.Net40.csproj" /p:Configuration=Release
xcopy "%project_bin_dir%\%project_name%.dll" lib\net40-client\
xcopy "%project_bin_dir%\ru-ru\%project_name%.resources.dll" lib\net40-client\ru-ru\
xcopy "%binaries_dir%\ClearScript.dll" lib\net40-client\

xcopy "%binaries_dir%\x86\ClearScriptV8-32.dll" runtimes\win7-x86\native\
xcopy "%binaries_dir%\x86\v8-ia32.dll" runtimes\win7-x86\native\
xcopy "%binaries_dir%\x64\ClearScriptV8-64.dll" runtimes\win7-x64\native\
xcopy "%binaries_dir%\x64\v8-x64.dll" runtimes\win7-x64\native\

copy "%licenses_dir%\clearscript-license.txt" clearscript-license.txt /Y
copy "%licenses_dir%\v8-license.txt" v8-license.txt /Y

%nuget_package_manager% pack "..\%project_name%\%project_name%.nuspec"