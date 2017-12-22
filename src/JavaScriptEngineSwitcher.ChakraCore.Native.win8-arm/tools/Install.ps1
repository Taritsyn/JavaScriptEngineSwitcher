param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimesDirectoryPath = Join-Path $installPath "runtimes"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileName = "ChakraCore.dll"

	$assemblyArmDestDirectoryPath = Join-Path $binDirectoryPath "arm"
	if (!(Test-Path $assemblyArmDestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assemblyArmDestDirectoryPath
	}

	$assemblyArmSourceFilePath = Join-Path $runtimesDirectoryPath ("win8-arm/native/" + $assemblyFileName)
	Copy-Item $assemblyArmSourceFilePath $assemblyArmDestDirectoryPath -Force
}