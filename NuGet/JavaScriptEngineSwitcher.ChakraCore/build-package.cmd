set project_name=JavaScriptEngineSwitcher.ChakraCore
set net4_project_source_dir=..\..\src\%project_name%.Net4
set net4_project_bin_dir=%net4_project_source_dir%\bin\Release
set dotnet_project_source_dir=..\..\src\%project_name%
set dotnet_project_bin_dir=%dotnet_project_source_dir%\bin\Release
set licenses_dir=..\..\Licenses
set nuget_package_manager=..\..\.nuget\nuget.exe

call "..\setup.cmd"

rmdir lib /Q/S

del chakra-samples-license.txt /Q/S
del jsrt-dotnet-license.txt /Q/S

%net40_msbuild% "%net4_project_source_dir%\%project_name%.Net40.csproj" /p:Configuration=Release
xcopy "%net4_project_bin_dir%\%project_name%.dll" lib\net40-client\
xcopy "%net4_project_bin_dir%\ru-ru\%project_name%.resources.dll" lib\net40-client\ru-ru\

%dotnet_cli% build "%dotnet_project_source_dir%" --framework net451 --configuration Release --no-dependencies --no-incremental
xcopy "%dotnet_project_bin_dir%\net451\%project_name%.dll" lib\net451\
xcopy "%dotnet_project_bin_dir%\net451\%project_name%.xml" lib\net451\
xcopy "%dotnet_project_bin_dir%\net451\ru-ru\%project_name%.resources.dll" lib\net451\ru-ru\

%dotnet_cli% build "%dotnet_project_source_dir%" --framework netstandard1.3 --configuration Release --no-dependencies --no-incremental
xcopy "%dotnet_project_bin_dir%\netstandard1.3\%project_name%.dll" lib\netstandard1.3\
xcopy "%dotnet_project_bin_dir%\netstandard1.3\%project_name%.xml" lib\netstandard1.3\
xcopy "%dotnet_project_bin_dir%\netstandard1.3\ru-ru\%project_name%.resources.dll" lib\netstandard1.3\ru-ru\

copy "%licenses_dir%\chakra-samples-license.txt" chakra-samples-license.txt /Y
copy "%licenses_dir%\jsrt-dotnet-license.txt" jsrt-dotnet-license.txt /Y

%nuget_package_manager% pack "..\%project_name%\%project_name%.nuspec"