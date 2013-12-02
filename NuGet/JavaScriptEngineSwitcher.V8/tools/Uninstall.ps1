param($installPath, $toolsPath, $package, $project)

$assemblyDirectoryName = "ClearScript.V8"

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyDirectoryPath = Join-Path $binDirectoryPath $assemblyDirectoryName
	
	if (Test-Path $assemblyDirectoryPath) {
		Remove-Item $assemblyDirectoryPath -Force -Recurse
	}
}