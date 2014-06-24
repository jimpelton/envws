param(
    [Parameter(Mandatory=$true)]
	[string]$outpath = "",
    [Parameter(Mandatory=$true)]
	[string]$namespace = "RepoVersion",
    [string]$workingDir = $pwd,
    [string]$class = "RepoVer"
)

Set-Location -Path $workingDir

[string]$a = svn info . | Where-Object {([string]$_).StartsWith("Revision:") -eq $True} | %{ echo ([string]$_).Split(":")[1].Trim() }
[Int32]$rev = 0
[Int32]::TryParse($a, [ref]$rev)
echo "namespace $namespace { public class $class { public static int VER=$a; } }" | Tee-Object -FilePath "$outpath\RepoVer.cs"
