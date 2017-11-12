set project_name=JavaScriptEngineSwitcher.V8
set project_source_dir=..\..\src\%project_name%
set project_bin_dir=%project_source_dir%\bin\Release
set lib_dir=..\..\lib\ClearScript
set licenses_dir=..\..\Licenses
set nuget_package_manager=..\..\.nuget\nuget.exe

call "..\setup.cmd"

rmdir lib /Q/S
del clearscript-license.txt /Q/S

%dotnet_cli% restore "%project_source_dir%"
%dotnet_cli% build "%project_source_dir%" --framework net45 --configuration Release --no-dependencies --no-incremental
xcopy "%project_bin_dir%\net45\%project_name%.dll" lib\net45\
xcopy "%project_bin_dir%\net45\%project_name%.xml" lib\net45\
xcopy "%project_bin_dir%\net45\ru-ru\%project_name%.resources.dll" lib\net45\ru-ru\
xcopy "%lib_dir%\lib\net40-client\ClearScript.dll" lib\net45\

copy "%licenses_dir%\clearscript-license.txt" clearscript-license.txt /Y

%nuget_package_manager% pack "..\%project_name%\%project_name%.nuspec"