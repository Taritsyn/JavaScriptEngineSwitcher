param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq 'Web Site') {
    $projectDir = $project.Properties.Item('FullPath').Value
    $assemblySourceFiles = Join-Path $installPath 'runtimes/win-x86/native/*.*'

    foreach ($assemblySourceFileInfo in Get-Item($assemblySourceFiles)) {
        $assemblyFile = Join-Path $projectDir "bin/x86/$($assemblySourceFileInfo.Name)"
        if (Test-Path $assemblyFile) {
            Remove-Item $assemblyFile -Force
        }
    }
}