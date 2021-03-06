# Used to copy the OState Envision SVN repo to our own Boise State repo.

$dirs = "ALPS","Libs","EnvEngine","DynamicVeg", "Flow", "Modeler", "Program",
"SpatialAllocator", "Sync", "Target", "Trigger", "SSTM", "Logbook","HBV",
"UltimateToolbox","GsTL","WebServices";

ForEach ($d in $dirs) {
$sdir = "D:\Documents\programming\envision\Envision.svn\src\"+$d
$ddir = "D:\Documents\programming\jpelton.svn\Envision\trunk\src\"+$d
$command = "robocopy "+$sdir+" "+$ddir+" * " +
 " /S /LOG+:D:\copylog.txt " +
 "/XD Release Debug x64 Win32 "+
 "/XF *.svn *.nc *.exe *.obj *.pdb *.htm *.html *.user /TEE"
iex $command;
 }
 
