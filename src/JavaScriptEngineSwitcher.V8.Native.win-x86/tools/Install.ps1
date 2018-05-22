param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$runtimesDirectoryPath = Join-Path $installPath "runtimes"
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assembly32FileNames = "ClearScriptV8-32.dll", "v8-base-ia32.dll", "v8-ia32.dll"

	$assembly32DestDirectoryPath = Join-Path $binDirectoryPath "x86"
	if (!(Test-Path $assembly32DestDirectoryPath)) {
		New-Item -ItemType Directory -Force -Path $assembly32DestDirectoryPath
	}

	foreach ($assembly32FileName in $assembly32FileNames) {
		$assembly32SourceFilePath = Join-Path $runtimesDirectoryPath ("win-x86/native/" + $assembly32FileName)
		Copy-Item $assembly32SourceFilePath $assembly32DestDirectoryPath -Force
	}
}