param($installPath, $toolsPath, $package, $project)

$assemblyDirectoryName = "Noesis.Javascript"
$assemblyName = "Noesis.Javascript"

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

	$assembly32Item = $assemblyDirectoryItem.ProjectItems.Item($assemblyName + ".x86.dll")
	$assembly32Item.Properties.Item("BuildAction").Value = 0
	$assembly32Item.Properties.Item("CopyToOutputDirectory").Value = 1

	$assembly64Item = $assemblyDirectoryItem.ProjectItems.Item($assemblyName + ".x64.dll")
	$assembly64Item.Properties.Item("BuildAction").Value = 0
	$assembly64Item.Properties.Item("CopyToOutputDirectory").Value = 1
}