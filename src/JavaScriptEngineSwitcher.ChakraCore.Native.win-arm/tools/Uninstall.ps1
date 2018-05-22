param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileName = "ChakraCore.dll"

	$assemblyArmDirectoryPath = Join-Path $binDirectoryPath "arm"
	$assemblyArmFilePath = Join-Path $assemblyArmDirectoryPath $assemblyFileName
	
	if (Test-Path $assemblyArmFilePath) {
		Remove-Item $assemblyArmFilePath -Force
	}
}