param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileName = "ChakraCore.dll"

	$assembly64DirectoryPath = Join-Path $binDirectoryPath "x64"
	$assembly64FilePath = Join-Path $assembly64DirectoryPath $assemblyFileName
	
	if (Test-Path $assembly64FilePath) {
		Remove-Item $assembly64FilePath -Force
	}
}