
$types = ".exe",".dll"

ForEach ($t in $types) 
{
    $sdir = "D:\Documents\programming\envision\Envision.svn\src\x64\Release\"
    copy $sdir+"*"+$t "D:\Release\"
}