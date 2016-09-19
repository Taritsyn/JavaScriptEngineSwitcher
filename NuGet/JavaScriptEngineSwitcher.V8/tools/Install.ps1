param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimesDirectoryPath = Join-Path $installPath "runtimes"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assembly32FileNames = "ClearScriptV8-32.dll", "v8-ia32.dll"
	$assembly64FileNames = "ClearScriptV8-64.dll", "v8-x64.dll"

	$assembly32DestDirectoryPath = Join-Path $binDirectoryPath "x86"
	if (!(Test-Path $assembly32DestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assembly32DestDirectoryPath
	}

	foreach ($assembly32FileName in $assembly32FileNames) {
		$assembly32SourceFilePath = Join-Path $runtimesDirectoryPath ("win7-x86/native/" + $assembly32FileName)
		Copy-Item $assembly32SourceFilePath $assembly32DestDirectoryPath -Force
	}

	$assembly64DestDirectoryPath = Join-Path $binDirectoryPath "x64"
	if (!(Test-Path $assembly64DestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assembly64DestDirectoryPath
	}

	foreach ($assembly64FileName in $assembly64FileNames) {
		$assembly64SourceFilePath = Join-Path $runtimesDirectoryPath ("win7-x64/native/" + $assembly64FileName)
		Copy-Item $assembly64SourceFilePath $assembly64DestDirectoryPath -Force
	}
}