param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assembly32FileNames = "ClearScriptV8-32.dll", "v8-ia32.dll"
	$assembly64FileNames = "ClearScriptV8-64.dll", "v8-x64.dll"

	$assembly32DirectoryPath = Join-Path $binDirectoryPath "x86"

	foreach ($assembly32FileName in $assembly32FileNames) {
		$assembly32FilePath = Join-Path $assembly32DirectoryPath $assembly32FileName
		if (Test-Path $assembly32FilePath) {
			Remove-Item $assembly32FilePath -Force
		}
	}

	$assembly64DirectoryPath = Join-Path $binDirectoryPath "x64"

	foreach ($assembly64FileName in $assembly64FileNames) {
		$assembly64FilePath = Join-Path $assembly64DirectoryPath $assembly64FileName
		if (Test-Path $assembly64FilePath) {
			Remove-Item $assembly64FilePath -Force
		}
	}
}