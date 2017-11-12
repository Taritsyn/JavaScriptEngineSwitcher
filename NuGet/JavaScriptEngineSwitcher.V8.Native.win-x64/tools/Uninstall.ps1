param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assembly64FileNames = "ClearScriptV8-64.dll", "v8-base-x64.dll", "v8-x64.dll"

	$assembly64DirectoryPath = Join-Path $binDirectoryPath "x64"

	foreach ($assembly64FileName in $assembly64FileNames) {
		$assembly64FilePath = Join-Path $assembly64DirectoryPath $assembly64FileName
		if (Test-Path $assembly64FilePath) {
			Remove-Item $assembly64FilePath -Force
		}
	}
}