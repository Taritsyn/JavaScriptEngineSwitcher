param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assembly32FileNames = "ClearScriptV8-32.dll", "v8-base-ia32.dll", "v8-ia32.dll"

	$assembly32DirectoryPath = Join-Path $binDirectoryPath "x86"

	foreach ($assembly32FileName in $assembly32FileNames) {
		$assembly32FilePath = Join-Path $assembly32DirectoryPath $assembly32FileName
		if (Test-Path $assembly32FilePath) {
			Remove-Item $assembly32FilePath -Force
		}
	}
}