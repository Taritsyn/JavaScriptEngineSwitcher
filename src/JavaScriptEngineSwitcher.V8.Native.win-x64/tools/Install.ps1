param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimeDirectoryPath = Join-Path $installPath "runtimes/win-x64/"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$managedAssemblyFileName = "ClearScriptV8-64.dll"
	$nativeAssemblyFileNames = "v8-base-x64.dll", "v8-x64.dll", "v8-zlib-x64.dll"

	$assemblyDestDirectoryPath = Join-Path $binDirectoryPath "x64"
	if (!(Test-Path $assemblyDestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assemblyDestDirectoryPath
	}

	$managedAssemblySourceFilePath = Join-Path $runtimeDirectoryPath ("lib/net45/" + $managedAssemblyFileName)
	Copy-Item $managedAssemblySourceFilePath $assemblyDestDirectoryPath -Force

	foreach ($nativeAssemblyFileName in $nativeAssemblyFileNames) {
		$nativeAssemblySourceFilePath = Join-Path $runtimeDirectoryPath ("native/" + $nativeAssemblyFileName)
		Copy-Item $nativeAssemblySourceFilePath $assemblyDestDirectoryPath -Force
	}
}