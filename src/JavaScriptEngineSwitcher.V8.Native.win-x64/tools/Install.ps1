param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimeDirectoryPath = Join-Path $installPath "runtimes/win-x64/"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileName = "ClearScriptV8-64.dll"

	$assemblyDestDirectoryPath = Join-Path $binDirectoryPath "x64"
	if (!(Test-Path $assemblyDestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assemblyDestDirectoryPath
	}

	$assemblySourceFilePath = Join-Path $runtimeDirectoryPath ("native/" + $assemblyFileName)
	Copy-Item $assemblySourceFilePath $assemblyDestDirectoryPath -Force
}