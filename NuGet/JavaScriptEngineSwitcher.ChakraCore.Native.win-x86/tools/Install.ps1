param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimesDirectoryPath = Join-Path $installPath "runtimes"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileName = "ChakraCore.dll"

	$assembly32DestDirectoryPath = Join-Path $binDirectoryPath "x86"
	if (!(Test-Path $assembly32DestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assembly32DestDirectoryPath
	}

	$assembly32SourceFilePath = Join-Path $runtimesDirectoryPath ("win-x86/native/" + $assemblyFileName)
	Copy-Item $assembly32SourceFilePath $assembly32DestDirectoryPath -Force
}