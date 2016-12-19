set project_name=JavaScriptEngineSwitcher.Vroom
set net4_project_source_dir=..\..\src\%project_name%.Net4
set net4_project_bin_dir=%net4_project_source_dir%\bin\Release
set dotnet_project_source_dir=..\..\src\%project_name%
set dotnet_project_bin_dir=%dotnet_project_source_dir%\bin\Release
set licenses_dir=..\..\Licenses
set nuget_package_manager=..\..\.nuget\nuget.exe

call "..\setup.cmd"

rmdir lib /Q/S

del vroomjs-core-license.txt /Q/S
del v8-license.txt /Q/S

%net40_msbuild% "%net4_project_source_dir%\%project_name%.Net40.csproj" /p:Configuration=Release
xcopy "%net4_project_bin_dir%\%project_name%.dll" lib\net40-client\
xcopy "%net4_project_bin_dir%\ru-ru\%project_name%.resources.dll" lib\net40-client\ru-ru\

%dotnet_cli% build "%dotnet_project_source_dir%" --framework net45 --configuration Release --no-dependencies --no-incremental
xcopy "%dotnet_project_bin_dir%\net45\%project_name%.dll" lib\net45\
xcopy "%dotnet_project_bin_dir%\net45\%project_name%.xml" lib\net45\
xcopy "%dotnet_project_bin_dir%\net45\ru-ru\%project_name%.resources.dll" lib\net45\ru-ru\

%dotnet_cli% build "%dotnet_project_source_dir%" --framework netstandard1.6 --configuration Release --no-dependencies --no-incremental
xcopy "%dotnet_project_bin_dir%\netstandard1.6\%project_name%.dll" lib\netstandard1.6\
xcopy "%dotnet_project_bin_dir%\netstandard1.6\%project_name%.xml" lib\netstandard1.6\
xcopy "%dotnet_project_bin_dir%\netstandard1.6\ru-ru\%project_name%.resources.dll" lib\netstandard1.6\ru-ru\

copy "%licenses_dir%\vroomjs-core-license.txt" vroomjs-core-license.txt /Y
copy "%licenses_dir%\v8-license.txt" v8-license.txt /Y

%nuget_package_manager% pack "..\%project_name%\%project_name%.nuspec"