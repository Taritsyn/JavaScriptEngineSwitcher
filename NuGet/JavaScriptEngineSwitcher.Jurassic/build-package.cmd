set project_name=JavaScriptEngineSwitcher.Jurassic
set project_source_dir=..\..\src\%project_name%
set project_bin_dir=%project_source_dir%\bin\Release
set licenses_dir=..\..\Licenses
set nuget_package_manager=..\..\.nuget\nuget.exe

call "..\setup.cmd"

rmdir lib /Q/S
del jurassic-license.txt /Q/S

%dotnet_cli% restore "%project_source_dir%"

%dotnet_cli% build "%project_source_dir%" --framework net40-client --configuration Release --no-dependencies --no-incremental
xcopy "%project_bin_dir%\net40-client\%project_name%.dll" lib\net40-client\
xcopy "%project_bin_dir%\net40-client\%project_name%.xml" lib\net40-client\
xcopy "%project_bin_dir%\net40-client\ru-ru\%project_name%.resources.dll" lib\net40-client\ru-ru\

%dotnet_cli% build "%project_source_dir%" --framework net45 --configuration Release --no-dependencies --no-incremental
xcopy "%project_bin_dir%\net45\%project_name%.dll" lib\net45\
xcopy "%project_bin_dir%\net45\%project_name%.xml" lib\net45\
xcopy "%project_bin_dir%\net45\ru-ru\%project_name%.resources.dll" lib\net45\ru-ru\

%dotnet_cli% build "%project_source_dir%" --framework netstandard2.0 --configuration Release --no-dependencies --no-incremental
xcopy "%project_bin_dir%\netstandard2.0\%project_name%.dll" lib\netstandard2.0\
xcopy "%project_bin_dir%\netstandard2.0\%project_name%.xml" lib\netstandard2.0\
xcopy "%project_bin_dir%\netstandard2.0\ru-ru\%project_name%.resources.dll" lib\netstandard2.0\ru-ru\

copy "%licenses_dir%\jurassic-license.txt" jurassic-license.txt /Y

%nuget_package_manager% pack "..\%project_name%\%project_name%.nuspec"