# Delete a service with the name provided on the command line.

param(
    [string]$name = ""
    )

if ($name -ne "") {
    $service = Get-WmiObject -Class Win32_Service -Filter "Name='$name'"
    $service.delete()
    }
