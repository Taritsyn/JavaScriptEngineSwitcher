param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileNames = "ClearScriptV8-64.dll", "v8-base-x64.dll", "v8-x64.dll", "v8-zlib-x64.dll"

	$assemblyDirectoryPath = Join-Path $binDirectoryPath "x64"

	foreach ($assemblyFileName in $assemblyFileNames) {
		$assemblyFilePath = Join-Path $assemblyDirectoryPath $assemblyFileName
		if (Test-Path $assemblyFilePath) {
			Remove-Item $assemblyFilePath -Force
		}
	}
}