param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimesDirectoryPath = Join-Path $installPath "runtimes"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assembly64FileNames = "ClearScriptV8-64.dll", "v8-base-x64.dll", "v8-x64.dll"

	$assembly64DestDirectoryPath = Join-Path $binDirectoryPath "x64"
	if (!(Test-Path $assembly64DestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assembly64DestDirectoryPath
	}

	foreach ($assembly64FileName in $assembly64FileNames) {
		$assembly64SourceFilePath = Join-Path $runtimesDirectoryPath ("win-x64/native/" + $assembly64FileName)
		Copy-Item $assembly64SourceFilePath $assembly64DestDirectoryPath -Force
	}
}