param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileNames = "ClearScriptV8-32.dll", "v8-base-ia32.dll", "v8-ia32.dll", "v8-zlib-ia32.dll"

	$assemblyDirectoryPath = Join-Path $binDirectoryPath "x86"

	foreach ($assemblyFileName in $assemblyFileNames) {
		$assemblyFilePath = Join-Path $assemblyDirectoryPath $assemblyFileName
		if (Test-Path $assemblyFilePath) {
			Remove-Item $assemblyFilePath -Force
		}
	}
}