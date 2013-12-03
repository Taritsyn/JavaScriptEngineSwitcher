param($installPath, $toolsPath, $package, $project)

$assemblyDirectoryName = "ClearScript.V8"
$assemblyFileNames = "ClearScriptV8-32.dll", "v8-ia32.dll", "ClearScriptV8-64.dll", "v8-x64.dll"

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyDirectoryPath = Join-Path $projectDirectoryPath $assemblyDirectoryName
	
	if (Test-Path $assemblyDirectoryPath) {
		if (!(Test-Path $binDirectoryPath)) {
			New-Item -ItemType Directory -Path $binDirectoryPath
		}
		
		Move-Item $assemblyDirectoryPath $binDirectoryPath -Force
	}
}
else {
	$assemblyDirectoryItem = $project.ProjectItems.Item($assemblyDirectoryName)
	
	foreach ($assemblyFileName in $assemblyFileNames) {
		$assemblyItem = $assemblyDirectoryItem.ProjectItems.Item($assemblyFileName)
		$assemblyItem.Properties.Item("BuildAction").Value = 0
		$assemblyItem.Properties.Item("CopyToOutputDirectory").Value = 2
	}
}