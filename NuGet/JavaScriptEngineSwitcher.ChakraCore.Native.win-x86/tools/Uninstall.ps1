param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site") {
	$projectDirectoryPath = $project.Properties.Item("FullPath").Value
	$binDirectoryPath = Join-Path $projectDirectoryPath "bin"
	$assemblyFileName = "ChakraCore.dll"

	$assembly32DirectoryPath = Join-Path $binDirectoryPath "x86"
	$assembly32FilePath = Join-Path $assembly32DirectoryPath $assemblyFileName

	if (Test-Path $assembly32FilePath) {
		Remove-Item $assembly32FilePath -Force
	}
}