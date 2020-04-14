param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimeDirectoryPath = Join-Path $installPath "runtimes/win-x86/"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$managedAssemblyFileName = "ClearScriptV8-32.dll"
	$nativeAssemblyFileNames = "v8-base-ia32.dll", "v8-ia32.dll", "v8-zlib-ia32.dll"

	$assemblyDestDirectoryPath = Join-Path $binDirectoryPath "x86"
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