set packages_directory="C:\Projects\JavaScriptEngineSwitcher\NuGet"

cd %packages_directory%\JavaScriptEngineSwitcher.Core
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.ConfigurationIntelliSense
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.Jint
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.Jurassic
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.Msie
call build-package.cmd

cd %packages_directory%\JavaScriptEngineSwitcher.V8
call build-package.cmd
