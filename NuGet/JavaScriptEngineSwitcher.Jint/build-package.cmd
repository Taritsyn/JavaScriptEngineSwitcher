set project_name=JavaScriptEngineSwitcher.Jint
set project_source_dir=..\..\src\%project_name%
set project_bin_dir=%project_source_dir%\bin\Release
set licenses_dir=..\..\Licenses
set nuget_package_manager=..\..\.nuget\nuget.exe

call "..\setup.cmd"

rmdir lib /Q/S
del jint-license.txt /Q/S

%net40_msbuild% "%project_source_dir%\%project_name%.Net40.csproj" /p:Configuration=Release
xcopy "%project_bin_dir%\%project_name%.dll" lib\net40-client\
xcopy "%project_bin_dir%\ru-ru\%project_name%.resources.dll" lib\net40-client\ru-ru\

%dotnet_cli% build "%project_source_dir%" --framework net451 --configuration Release --no-dependencies --no-incremental
xcopy "%project_bin_dir%\net451\%project_name%.dll" lib\net451\
xcopy "%project_bin_dir%\net451\%project_name%.xml" lib\net451\
xcopy "%project_bin_dir%\net451\ru-ru\%project_name%.resources.dll" lib\net451\ru-ru\

copy "%licenses_dir%\jint-license.txt" jint-license.txt /Y

%nuget_package_manager% pack "..\%project_name%\%project_name%.nuspec"