param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimeDirectoryPath = Join-Path $installPath "runtimes/win-x86/"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileName = "ChakraCore.dll"

	$assemblyDestDirectoryPath = Join-Path $binDirectoryPath "x86"
	if (!(Test-Path $assemblyDestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assemblyDestDirectoryPath
	}

	$assemblySourceFilePath = Join-Path $runtimeDirectoryPath ("native/" + $assemblyFileName)
	Copy-Item $assemblySourceFilePath $assemblyDestDirectoryPath -Force
}