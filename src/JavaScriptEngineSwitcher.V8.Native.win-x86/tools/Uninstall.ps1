param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileName = "ClearScriptV8-32.dll"

	$assemblyDirectoryPath = Join-Path $binDirectoryPath "x86"
	$assemblyFilePath = Join-Path $assemblyDirectoryPath $assemblyFileName

	if (Test-Path $assemblyFilePath) {
		Remove-Item $assemblyFilePath -Force
	}
}